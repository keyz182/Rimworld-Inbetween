using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Inbetween.Inbetween;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.Profile;

namespace Inbetween.HarmonyPatches;

[HarmonyPatch(typeof(Game))]
public static class Game_Patch
{
    [HarmonyPatch(nameof(Game.InitNewGame))]
    [HarmonyPrefix]
    public static bool InitNewGame_Patch(Game __instance)
    {
        if (!Current.Game.GetComponent<InbetweenGameComponent>().InbetweenQuickplayMode)
            return true;

        Log.Message("Initializing new game with mods:\n" + LoadedModManager.RunningMods
            .Select(mod => mod.PackageIdPlayerFacing + (!mod.ModMetaData.VersionCompatible ? " (incompatible version)" : "")).ToLineList("  - "));

        FieldInfo mapsField = AccessTools.Field(typeof(Game), "maps");
        FieldInfo initDataField = AccessTools.Field(typeof(Game), "initData");
        FieldInfo infoField = AccessTools.Field(typeof(Game), "info");
        FieldInfo worldIntField = AccessTools.Field(typeof(Game), "worldInt");

        List<Map> maps = (List<Map>) mapsField.GetValue(__instance);
        GameInitData initData = (GameInitData) initDataField.GetValue(__instance);
        GameInfo info = (GameInfo) infoField.GetValue(__instance);
        World worldInt = (World) worldIntField.GetValue(__instance);

        if (maps.Any())
            Log.Error("Called InitNewGame() but there already is a map. There should be 0 maps...");
        else if (initData == null)
        {
            Log.Error("Called InitNewGame() but init data is null. Create it first.");
        }
        else
        {
            MemoryUtility.UnloadUnusedUnityAssets();
            DeepProfiler.Start(nameof(InitNewGame_Patch));
            try
            {
                Current.ProgramState = ProgramState.MapInitializing;
                IntVec3 mapSize = new IntVec3(initData.mapSize, 1, initData.mapSize);

                List<Settlement> settlements = Find.WorldObjects.Settlements;

                Settlement parent = settlements.FirstOrDefault(s => s.Faction == Faction.OfPlayer);

                if (parent == null)
                    Log.Error("Could not generate starting map because there is no any player faction base.");

                __instance.tickManager.gameStartAbsTick = GenTicks.ConfiguredTicksAbsAtGameStart;
                info.startingTile = initData.startingTile;
                info.startingAndOptionalPawns = initData.startingAndOptionalPawns;

                Map map = MapGenerator.GenerateMap(mapSize, parent, InbetweenDefOf.IB_Empty); //, parent.ExtraGenStepDefs);

                worldInt.info.initialMapSize = mapSize;
                if (initData.permadeath)
                {
                    info.permadeathMode = true;
                    info.permadeathModeUniqueName = PermadeathModeUtility.GeneratePermadeathSaveName();
                }

                PawnUtility.GiveAllStartingPlayerPawnsThought(ThoughtDefOf.NewColonyOptimism);
                __instance.FinalizeInit();
                Current.Game.CurrentMap = map;
                Find.CameraDriver.JumpToCurrentMapLoc(MapGenerator.PlayerStartSpot);
                Find.CameraDriver.ResetSize();
                if (Prefs.PauseOnLoad && initData.startedFromEntry)
                    LongEventHandler.ExecuteWhenFinished(() =>
                    {
                        __instance.tickManager.DoSingleTick();
                        __instance.tickManager.CurTimeSpeed = TimeSpeed.Paused;
                    });

                foreach (Pawn pawm in Find.GameInitData.startingAndOptionalPawns)
                {
                    pawm.SetFaction(Faction.OfPlayer);
                }

                Find.Scenario.PostGameStart();
                __instance.history.FinalizeInit();
                ResearchUtility.ApplyPlayerStartingResearch();
                GameComponentUtility.StartedNewGame();
                initDataField.SetValue(__instance, null);
                infoField.SetValue(__instance, info);
            }
            finally
            {
                DeepProfiler.End();
            }
        }

        return false;
    }
}

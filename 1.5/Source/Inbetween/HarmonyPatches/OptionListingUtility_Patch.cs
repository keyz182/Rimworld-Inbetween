using System.Collections.Generic;
using HarmonyLib;
using Inbetween.Inbetween;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace Inbetween.HarmonyPatches;

[HarmonyPatch(typeof(OptionListingUtility))]
public static class OptionListingUtility_Patch
{
    public static void SetupForQuickIBPlay()
    {
        Current.ProgramState = ProgramState.Entry;
        Current.Game = new Game();
        Current.Game.GetComponent<InbetweenGameComponent>().InbetweenQuickplayMode = true;
        Current.Game.InitData = new GameInitData();
        Current.Game.Scenario = InbetweenDefOf.IB_Quickstart.scenario;
        Find.Scenario.PreConfigure();
        Current.Game.storyteller = new Storyteller(StorytellerDefOf.Cassandra, DifficultyDefOf.Rough);
        Current.Game.World = WorldGenerator.GenerateWorld(
            0.05f,
            GenText.RandomSeedString(),
            OverallRainfall.Normal,
            OverallTemperature.Normal,
            OverallPopulation.AlmostNone,
            new List<FactionDef>()
            {
                FactionDefOf.OutlanderRefugee
            });
        Find.GameInitData.ChooseRandomStartingTile();
        Find.GameInitData.mapSize = 75;
        Find.Scenario.PostIdeoChosen();
    }

    [HarmonyPatch(nameof(OptionListingUtility.DrawOptionListing))]
    [HarmonyPrefix]
    public static void DrawOptionListing_Patch(Rect rect, ref List<ListableOption> optList)
    {
        if (!optList.Any(l => l.label == "Tutorial".Translate() || l.label == "Options".Translate()))
            return;

        ListableOption newOpt = new ListableOption("Inbetween_MainMenu".Translate(),
            () => LongEventHandler.QueueLongEvent(() =>
                {
                    SetupForQuickIBPlay();
                    PageUtility.InitGameStart();
                },
                "Inbetween_GeneratingInbetween",
                true,
                GameAndMapInitExceptionHandlers.ErrorWhileGeneratingMap
                )
            );

        optList.Insert(0, newOpt);
    }
}

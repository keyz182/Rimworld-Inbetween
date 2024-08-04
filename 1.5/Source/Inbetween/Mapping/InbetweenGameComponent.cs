using System;
using System.Collections.Generic;
using System.Linq;
using Inbetween.Buildings;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Inbetween.Mapping;

public class InbetweenGameComponent : GameComponent
{
    // Game component to manage the inbetween lifecycle

    public bool InbetweenQuickplayMode = false;
    public static bool CurrentlyGenerating = false;
    public static List<Action> GenerationPostActions = new List<Action>();

    // currently held pocket maps
    public List<Map> Maps = new List<Map>();

    public IEnumerable<InbetweenZoneMapComponent> MapComponents =>
        Maps.Where(map => map.components.Any(c => c is InbetweenZoneMapComponent)).Select(map => map.GetComponent<InbetweenZoneMapComponent>());

    // the root door - i.e. the door not on a pocket map
    public Building_InbetweenDoor RootDoor;

    public Map RootMap;

    public InbetweenZoneMapComponent RootMapComponent
    {
        get
        {
            InbetweenZoneMapComponent mapComp = RootMap.GetComponent<InbetweenZoneMapComponent>();

            if (mapComp == null)
            {
                mapComp = new InbetweenZoneMapComponent(RootMap);
                mapComp.Root = true;
                RootMap.components.Add(mapComp);
            }

            return mapComp;
        }
    }

    public int MaxLoadedZones => InbetweenMod.settings.MaxLoadedZones;

    public InbetweenGameComponent() { }

    public InbetweenGameComponent(Game game) { }

    public override void ExposeData()
    {
        Scribe_Collections.Look(ref Maps, "Maps", LookMode.Reference);
        Scribe_References.Look(ref RootDoor, "RootDoor");
        Scribe_References.Look(ref RootMap, "RootMap");
    }

    public bool CanDoNextMap(Building_IBPortal door)
    {
        if (door is Building_ReturnDoor)
        {
            return true;
        }

        // If there's less than 3 pocket maps, we can safely generate the next
        if (Maps.Count < MaxLoadedZones)
        {
            return true;
        }

        // If the door isn't on the last map, we can safely travel to the next
        if (door.Map != Maps.LastOrDefault())
        {
            return true;
        }

        // If there are colonists on any previous maps, no new maps
        return !Maps.GetRange(0, Maps.Count - (MaxLoadedZones - 1)).Any(map => map.mapPawns.AnyColonistSpawned);
    }

    public InbetweenZoneDef NextMapGen()
    {
        // Grabs a random inbetween zone
        // TODO: weighted random based on difficulties and similar?
        return DefDatabase<InbetweenZoneDef>.GetRandom();
    }

    public void UpdateMapComps()
    {
        // Handle first door

        // Point the root door to the now-oldest map in the chain
        Map map = Maps.FirstOrDefault();
        RootMapComponent.NextMap = map;

        if (map != null)
        {
            InbetweenZoneMapComponent comp = map.GetComponent<InbetweenZoneMapComponent>();
            comp.LastMap = RootMap;
            if (Maps.Count > 1)
            {
                comp.NextMap = Maps[1];
            }
        }

        for (int i = 1; i < Maps.Count; i++)
        {
            Map current = Maps[i];
            InbetweenZoneMapComponent comp = current.GetComponent<InbetweenZoneMapComponent>();

            comp.LastMap = Maps[i - 1];
            if (i + 1 < Maps.Count)
            {
                comp.NextMap = Maps[i + 1];
            }
        }
    }

    public void TryGenerateNextMap(Building_InbetweenDoor door, Action callback)
    {
        ModLog.Log("Requesting generation of Pocket Map");
        if (CurrentlyGenerating)
        {
            ModLog.Error("Attempted to generate maps simultaneously!");
            GenerationPostActions.Add(callback);
            return;
        }

        LongEventHandler.QueueLongEvent(() =>
            {
                TryGenerateNextMapInt(door);
            },
            "Inbetween_GeneratingInbetween",
            true,
            delegate
            {
                DelayedErrorWindowRequest.Add("ErrorWhileGeneratingMap".Translate(), "ErrorWhileGeneratingMapTitle".Translate());
                Scribe.ForceStop();
                GenScene.GoToMainMenu();
            },
            true,
            delegate
            {
                foreach (Action action in GenerationPostActions)
                {
                    action();
                }

                GenerationPostActions.Clear();
            });
    }

    public bool TryGenerateNextMapInt(Building_InbetweenDoor door)
    {
        ModLog.Log("Generating Pocket Map");
        if (CurrentlyGenerating)
        {
            ModLog.Error("Aborted attempted to generate maps simultaneously!");
            return false;
        }

        CurrentlyGenerating = true;
        // we can't generate yet
        if (!CanDoNextMap(door))
        {
            ModLog.Error("Aborted attempted to generate maps due to CanDoNextMap being false!");
            return false;
        }

        if (RootMap == null && !Find.World.pocketMaps.Any(p => p.Map == door.Map))
        {
            ModLog.Log($"Set new root map {door.Map}");
            RootMap = door.Map;
        }

        Map nextSubMap = null;

        try
        {
            List<GenStepWithParams> extraSteps = new List<GenStepWithParams>();
            InbetweenZoneDef newMapDef = NextMapGen();

            // Generate the pocket map
            PocketMapParent parent = WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.PocketMap) as PocketMapParent;
            if (parent == null)
            {
                ModLog.Error("Aborted attempted to generate maps due MakeWorldObject failing!");
                return false;
            }

            parent.sourceMap = RootMap;
            IntVec3 mapSize = new IntVec3(50, 1, 50);
            MapGeneratorDef gen = newMapDef.mapGenerator;

            nextSubMap = MapGenerator.GenerateMap(mapSize, parent, gen, extraSteps, isPocketMap: true);
            Find.World.pocketMaps.Add(parent);

            // if the map has no InbetweenZoneMapComponent, add one
            InbetweenZoneMapComponent mapComp;
            if ((mapComp = nextSubMap.GetComponent<InbetweenZoneMapComponent>()) == null)
            {
                mapComp = new InbetweenZoneMapComponent(nextSubMap);
                nextSubMap.components.Add(mapComp);
            }

            mapComp.InbetweenZoneDef = newMapDef;
            mapComp.LastMap = Maps.LastOrDefault();

            // Clean up old maps if necessary
            if (Maps.Count >= MaxLoadedZones)
            {
                Map poppedMap = Maps.PopFront();
                PocketMapUtility.DestroyPocketMap(poppedMap);
                Current.Game.Maps.Remove(poppedMap);
                poppedMap.Dispose();
            }

            Maps.Add(nextSubMap);
            UpdateMapComps();

            door.Open = true;
            door.Map.mapDrawer.MapMeshDirty(door.Position, MapMeshFlagDefOf.Buildings);
        }
        catch (Exception e)
        {
            ModLog.Error("Unknown error generating pocket map -> " + e.ToStringSafe());

            if (nextSubMap != null)
            {
                PocketMapUtility.DestroyPocketMap(nextSubMap);
                Current.Game.Maps.Remove(nextSubMap);
                nextSubMap.Dispose();
            }

            throw;
        }
        finally
        {
            CurrentlyGenerating = false;
        }

        return true;
    }
}

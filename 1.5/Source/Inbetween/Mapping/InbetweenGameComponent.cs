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

    // the root door - i.e. the door not on a pocket map
    public Building_InbetweenDoor RootDoor;

    public Map RootMap;

    public int MaxLoadedZones => InbetweenMod.settings.MaxLoadedZones;

    public InbetweenGameComponent() { }

    public InbetweenGameComponent(Game game) { }

    public override void ExposeData()
    {
        Scribe_Collections.Look(ref Maps, "Maps", LookMode.Reference);
        Scribe_References.Look(ref RootDoor, "RootDoor");
        Scribe_References.Look(ref RootMap, "RootMap");
    }

    public bool CanDoNextMap(Building_InbetweenDoor door)
    {
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

    public void UpdateRoot()
    {
        // Point the root door to the now-oldest map in the chain
        Building_ReturnDoor retDoor = Maps.FirstOrDefault()?.listerThings.ThingsOfDef(InbetweenDefOf.IB_ReturnDoor).First() as Building_ReturnDoor;

        if (retDoor == null)
        {
            return;
        }

        RootDoor.ReturnDoor = retDoor;
        RootDoor.nextMap = Maps.FirstOrDefault();
        retDoor.inbetweenDoor = RootDoor;
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
            ModLog.Error("Attempted to generate maps simultaneously!");
            return false;
        }

        CurrentlyGenerating = true;
        // we can't generate yet
        if (!CanDoNextMap(door))
        {
            return false;
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
                return false;
            }

            parent.sourceMap = RootMap;
            IntVec3 mapSize = new IntVec3(50, 1, 50);
            MapGeneratorDef gen = newMapDef.mapGenerator;

            nextSubMap = MapGenerator.GenerateMap(mapSize, parent, gen, extraSteps, isPocketMap: true);
            Find.World.pocketMaps.Add(parent);

            // if the map has no InbetweenZoneMapComponent, add one
            if (nextSubMap.GetComponent<InbetweenZoneMapComponent>() == null)
            {
                InbetweenZoneMapComponent ibzmc = new InbetweenZoneMapComponent(nextSubMap);
                ibzmc._inbetweenZoneDef = newMapDef;
                nextSubMap.components.Add(ibzmc);
            }

            // set the new map as the doors next map
            door.nextMap = nextSubMap;

            // Find the generated return door to link them
            if (nextSubMap.listerThings.ThingsOfDef(InbetweenDefOf.IB_ReturnDoor).FirstOrDefault() is Building_ReturnDoor ReturnDoor)
            {
                ReturnDoor.inbetweenDoor = door;
            }

            // Clean up old maps
            if (Maps.Count >= MaxLoadedZones)
            {
                Map poppedMap = Maps.PopFront();

                // poppedMap.AllCells.ToList().ForEach(c => poppedMap.thingGrid.ThingsAt(c).ToList().ForEach(t => t.Destroy()));

                PocketMapUtility.DestroyPocketMap(poppedMap);
                Current.Game.Maps.Remove(poppedMap);
                poppedMap.Dispose();
                UpdateRoot();
            }

            door.Open = true;

            Maps.Add(nextSubMap);
        }
        catch (Exception e)
        {
            ModLog.Error("Unknown error generating pocket map -> " + e.ToStringSafe());

            if (nextSubMap != null)
            {
                // Clean up if we failed to properly do maps
                door.nextMap = null;
                door.ReturnDoor = null;
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

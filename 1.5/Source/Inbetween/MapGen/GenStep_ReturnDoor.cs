using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace Inbetween.MapGen;

public class GenStep_ReturnDoor : GenStep_Scatterer
{
    // Place a return door on the map
    private const int Size = 60;

    public override int SeedPart => 473957245;

    public override void Generate(Map map, GenStepParams parms)
    {
        count = 1;
        nearMapCenter = true;
        base.Generate(map, parms);
    }

    private bool CanPlaceDoorAt(IntVec3 cell, Map map)
    {
        CellRect cellRect = GenAdj.OccupiedRect(cell, Rot4.North, InbetweenDefOf.IB_ReturnDoor.Size).ExpandedBy(1);
        foreach (IntVec3 c in cellRect)
        {
            if (c.GetEdifice(map) != null)
            {
                return false;
            }

            if (c.GetThingList(map).Any())
            {
                return false;
            }
        }

        return true;
    }

    protected override bool CanScatterAt(IntVec3 c, Map map)
    {
        if (!base.CanScatterAt(c, map) || !c.Standable(map) || !CanPlaceDoorAt(c, map))
        {
            return false;
        }

        return true;
        Thing door = map.listerThings.ThingsOfDef(InbetweenDefOf.IB_Door).First();

        if (door == null)
        {
            return true;
        }

        IntVec3 c1 = ThingUtility.InteractionCellWhenAt(InbetweenDefOf.IB_ReturnDoor, c, InbetweenDefOf.IB_ReturnDoor.defaultPlacingRot, map);
        IntVec3 c2 = ThingUtility.InteractionCellWhenAt(InbetweenDefOf.IB_Door, door.Position, InbetweenDefOf.IB_Door.defaultPlacingRot, map);

        return map.reachability.CanReach(c1, new LocalTargetInfo(c2), PathEndMode.ClosestTouch, TraverseMode.PassAllDestroyableThings);
    }

    protected override void ScatterAt(IntVec3 c, Map map, GenStepParams parms, int stackCount = 1)
    {
        GenSpawn.Spawn(ThingMaker.MakeThing(InbetweenDefOf.IB_ReturnDoor, ThingDefOf.WoodLog), c, map);
    }
}

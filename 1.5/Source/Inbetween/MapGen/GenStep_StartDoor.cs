using RimWorld;
using Verse;

namespace Inbetween.MapGen;

public class GenStep_StartDoor : GenStep_Scatterer
{
    // Place a start door on the map
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
    }

    protected override void ScatterAt(IntVec3 c, Map map, GenStepParams parms, int stackCount = 1)
    {
        GenSpawn.Spawn(ThingMaker.MakeThing(InbetweenDefOf.IB_Door, ThingDefOf.WoodLog), c, map);
    }
}

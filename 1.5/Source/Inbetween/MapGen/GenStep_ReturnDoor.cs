using RimWorld;
using Verse;

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

    protected override bool CanScatterAt(IntVec3 c, Map map)
    {
        if (!base.CanScatterAt(c, map) || !c.Standable(map) || c.Roofed(map))
            return false;

        IntVec3 c1 = ThingUtility.InteractionCellWhenAt(InbetweenDefOf.IB_ReturnDoor, c, InbetweenDefOf.IB_ReturnDoor.defaultPlacingRot, map);
        return map.reachability.CanReachMapEdge(c1, TraverseParms.For(TraverseMode.PassDoors));
    }

    protected override void ScatterAt(IntVec3 c, Map map, GenStepParams parms, int stackCount = 1)
    {
        GenSpawn.Spawn(ThingMaker.MakeThing(InbetweenDefOf.IB_ReturnDoor, ThingDefOf.WoodLog), c, map);
    }
}

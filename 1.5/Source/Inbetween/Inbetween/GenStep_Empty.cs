using RimWorld;
using Verse;

namespace Inbetween.Inbetween;

public class GenStep_Empty : GenStep
{
    public override int SeedPart => 3713736;

    public override void Generate(Map map, GenStepParams parms)
    {
        RimWorld.BaseGen.BaseGen.Generate();
        map.terrainGrid.ResetGrids();
        foreach (IntVec3 allCell in map.AllCells)
            map.terrainGrid.SetTerrain(allCell, TerrainDefOf.Concrete);
    }
}

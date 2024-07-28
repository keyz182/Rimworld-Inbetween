using RimWorld;
using Verse;

namespace Inbetween.MapGen;

public class GenStep_TerrainEmpty : GenStep
{
    // Generate empty terrain
    public override void Generate(Map map, GenStepParams parms)
    {
        TerrainGrid terrainGrid = map.terrainGrid;
        MapGenFloatGrid fertility = MapGenerator.Fertility;
        foreach (IntVec3 allCell in map.AllCells)
        {
            terrainGrid.SetUnderTerrain(allCell, TerrainDefOf.Sand);
            TerrainDef terrainDef = TerrainThreshold.TerrainAtValue(map.Biome.terrainsByFertility, fertility[allCell]);
            terrainGrid.SetTerrain(allCell, terrainDef);
        }
    }

    public override int SeedPart => 26262235;
}

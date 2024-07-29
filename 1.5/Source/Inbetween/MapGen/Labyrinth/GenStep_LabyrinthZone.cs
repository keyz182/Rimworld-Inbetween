using HarmonyLib;
using RimWorld;
using Verse;

namespace Inbetween.MapGen.Labyrinth;

public class GenStep_LabyrinthZone : GenStep
{
    public override int SeedPart => 1217995;

    private LayoutStructureSketch structureSketch;


    public override void Generate(Map map, GenStepParams parms)
    {
        if (!ModLister.CheckAnomaly("Labyrinth"))
        {
            return;
        }

        foreach (IntVec3 allCell in map.AllCells)
        {
            map.terrainGrid.SetTerrain(allCell, TerrainDefOf.GraySurface);
        }

        StructureGenParams structureParams = new StructureGenParams { size = new IntVec2(map.Size.x, map.Size.z) };

        LayoutWorker worker = InbetweenDefOf.IB_LabyrinthZone.Worker;

        structureSketch = worker.GenerateStructureSketch(structureParams);

        worker.Spawn(structureSketch, map, IntVec3.Zero, null, null, false);
        worker.FillRoomContents(structureSketch, map);
        map.layoutStructureSketch = structureSketch;
        LabyrinthZoneMapComponent component = map.GetComponent<LabyrinthZoneMapComponent>();

        MapGenerator.PlayerStartSpot = IntVec3.Zero;

        AccessTools.Method(typeof(FogGrid), "SetAllFogged").Invoke(map.fogGrid, []);
    }
}

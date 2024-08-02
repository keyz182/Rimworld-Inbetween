using System.Linq;
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

        int num = 10;
        do
        {
            structureSketch = worker.GenerateStructureSketch(structureParams);
        } while (
            (
                structureSketch == null ||
                !structureSketch.structureLayout.HasRoomWithDef(InbetweenDefOf.IB_LabyrinthReturnDoor) ||
                !structureSketch.structureLayout.HasRoomWithDef(InbetweenDefOf.IB_LabyrinthDoor)
            ) && num-- > 0);

        if (num == 0)
        {
            ModLog.Error("Failed to generate labyrinth zone, guard exceeded. Check layout worker for errors placing minimum rooms");
        }
        else
        {
            worker.Spawn(structureSketch, map, IntVec3.Zero, null, null, false);
            worker.FillRoomContents(structureSketch, map);
            map.layoutStructureSketch = structureSketch;
            LabyrinthZoneMapComponent component = map.GetComponent<LabyrinthZoneMapComponent>();

            MapGenerator.PlayerStartSpot = IntVec3.Zero;

            AccessTools.Method(typeof(FogGrid), "SetAllFogged").Invoke(map.fogGrid, []);

            LayoutRoom returnroom = structureSketch.layoutSketch.layout.Rooms.First(r => r.requiredDef == InbetweenDefOf.IB_LabyrinthReturnDoor);

            foreach (IntVec3 cell in returnroom.Cells)
            {
                map.fogGrid.Unfog(cell);
            }
        }
    }
}

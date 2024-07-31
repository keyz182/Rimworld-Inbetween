using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Inbetween.MapGen.Labyrinth;

public class LayoutWorkerLabyrinthZone : LayoutWorkerLabyrinth
{
    private static readonly IntRange RoomSizeRange = new IntRange(8, 12);
    private static readonly IntRange LShapeRoomRange = new IntRange(6, 12);
    private static readonly IntRange RoomRange = new IntRange(32, 48);
    private const int Border = 2;
    private const int CorridorInflation = 3;
    private const int ObeliskRoomSize = 19;
    private static readonly PriorityQueue<IntVec3, int> openSet = new PriorityQueue<IntVec3, int>();
    private static readonly Dictionary<IntVec3, IntVec3> cameFrom = new Dictionary<IntVec3, IntVec3>();
    private static readonly Dictionary<IntVec3, int> gScore = new Dictionary<IntVec3, int>();
    private static readonly Dictionary<IntVec3, int> fScore = new Dictionary<IntVec3, int>();
    private static readonly List<IntVec3> toEnqueue = new List<IntVec3>();
    private static readonly List<IntVec3> tmpCells = new List<IntVec3>();

    public LayoutWorkerLabyrinthZone(LayoutDef def)
        : base(def)
    {
    }

    protected override LayoutSketch GenerateSketch(StructureGenParams parms)
    {
        if (!ModLister.CheckAnomaly("Labyrinth"))
        {
            return null;
        }

        LayoutSketch sketch = new LayoutSketch
        {
            wall = ThingDefOf.GrayWall,
            door = ThingDefOf.GrayDoor,
            floor = TerrainDefOf.GraySurface,
            wallStuff = ThingDefOf.LabyrinthMatter,
            doorStuff = ThingDefOf.LabyrinthMatter
        };
        CellRect rect = new CellRect(0, 0, parms.size.x, parms.size.z);
        FillEdges(rect, sketch);
        CellRect cellRect = rect.ContractedBy(2);
        parms.size = new IntVec2(cellRect.Width, cellRect.Height);

        sketch.layout = GenerateLabyrinth(parms);
        sketch.FlushLayoutToSketch(new IntVec3(2, 0, 2));

        return sketch;
    }

    public StructureLayout GenerateLabyrinth(StructureGenParams parms)
    {
        StructureLayout layout = new StructureLayout();
        CellRect cellRect = new CellRect(0, 0, parms.size.x, parms.size.z);
        layout.Init(cellRect);

        LayoutRoom returnRoom = PlaceReturnDoorRoom(cellRect, layout);
        LayoutRoom exitRoom = PlaceExitDoorRoom(cellRect, layout);

        ScatterLRooms(cellRect, layout);
        ScatterSquareRooms(cellRect, layout);
        GenerateGraphs(layout);
        layout.FinalizeRooms(false);
        CreateDoors(layout);
        CreateCorridorsAStar(layout);
        FillEmptySpaces(layout);

        return layout;
    }

    private static LayoutRoom PlaceReturnDoorRoom(CellRect size, StructureLayout layout)
    {
        //Randomize size
        int maxWidth = RoomSizeRange.RandomInRange;
        int maxHeight = RoomSizeRange.RandomInRange;
        int minX = Rand.Range(0, size.Width - maxWidth);
        int minZ = Rand.Range(0, size.Height - maxHeight);
        int width = LShapeRoomRange.RandomInRange;
        int height = LShapeRoomRange.RandomInRange;
        for (; Mathf.Abs(width - maxWidth) <= 2; width = LShapeRoomRange.RandomInRange) { }

        for (; Mathf.Abs(height - maxHeight) <= 2; height = LShapeRoomRange.RandomInRange) { }

        CellRect rect = new CellRect(minX, minZ, maxWidth, maxHeight);
        CellRect cellRect = !Rand.Bool ? new CellRect(rect.minX - width, rect.minZ, width + 1, height) : new CellRect(rect.maxX, rect.maxZ - height + 1, width, height);


        LayoutRoom layoutRoom = layout.AddRoom(new List<CellRect> { cellRect });
        layoutRoom.requiredDef = InbetweenDefOf.IB_LabyrinthReturnDoor;
        layoutRoom.defs = new List<LayoutRoomDef>();
        layoutRoom.entryCells = new List<IntVec3>();
        layoutRoom.entryCells.AddRange(cellRect.GetCenterCellsOnEdge(Rot4.North, 2));
        layoutRoom.entryCells.AddRange(cellRect.GetCenterCellsOnEdge(Rot4.East, 2));
        layoutRoom.entryCells.AddRange(cellRect.GetCenterCellsOnEdge(Rot4.South, 2));
        layoutRoom.entryCells.AddRange(cellRect.GetCenterCellsOnEdge(Rot4.West, 2));

        return layoutRoom;
    }

    private static LayoutRoom PlaceExitDoorRoom(CellRect size, StructureLayout layout)
    {
        LayoutRoom layoutRoom = null;

        for (int index = 0; index < 10; ++index)
        {
            int width = RoomSizeRange.RandomInRange;
            int height = RoomSizeRange.RandomInRange;
            CellRect rect = new CellRect(Rand.Range(0, size.Width - width), Rand.Range(0, size.Height - height), width, height);
            if (!OverlapsWithAnyRoom(layout, rect))
            {
                layoutRoom = layout.AddRoom(new List<CellRect> { rect });
                break;
            }
        }

        if (layoutRoom == null)
        {
            throw new Exception("Failed to generate exit room in 10 tries");
        }

        layoutRoom.requiredDef = InbetweenDefOf.IB_LabyrinthDoor;
        layoutRoom.defs = new List<LayoutRoomDef>();

        return layoutRoom;
    }


    private static List<LayoutRoom> GetDoorableRooms(StructureLayout layout, LayoutRoom returnRoom)
    {
        List<LayoutRoom> list = new List<LayoutRoom>();
        list.AddRange(layout.Rooms);

        list.Remove(layout.Rooms.First());
        foreach (LayoutRoom logicalRoomConnection in layout.GetLogicalRoomConnections(returnRoom))
        {
            if (list.Contains(logicalRoomConnection))
            {
                list.Remove(logicalRoomConnection);
                foreach (LayoutRoom logicalRoomConnection2 in layout.GetLogicalRoomConnections(logicalRoomConnection))
                {
                    if (list.Contains(logicalRoomConnection2))
                    {
                        list.Remove(logicalRoomConnection2);
                    }
                }
            }
        }

        for (int index = list.Count - 1; index >= 0; index--)
        {
            if (list[index].defs != null && list[index].defs.Any(d => !d.isValidPlayerSpawnRoom))
            {
                list.RemoveAt(index);
                break;
            }
        }

        if (list.Empty())
        {
            list.Clear();
            list.AddRange(layout.Rooms);
            list.Remove(returnRoom);
        }

        return list;
    }

    private static bool OverlapsWithAnyRoom(StructureLayout layout, CellRect rect)
    {
        return (bool) AccessTools.Method(typeof(LayoutWorkerLabyrinth), "OverlapsWithAnyRoom", [typeof(StructureLayout), typeof(CellRect)]).Invoke(null, [layout, rect]);
    }

    private static void FillEmptySpaces(StructureLayout layout)
    {
        AccessTools.Method(typeof(LayoutWorkerLabyrinth), "FillEmptySpaces", [typeof(StructureLayout)]).Invoke(null, [layout]);
    }

    private static void GenerateGraphs(StructureLayout layout)
    {
        AccessTools.Method(typeof(LayoutWorkerLabyrinth), "GenerateGraphs", [typeof(StructureLayout)]).Invoke(null, [layout]);
    }

    private static void ScatterLRooms(CellRect size, StructureLayout layout)
    {
        AccessTools.Method(typeof(LayoutWorkerLabyrinth), "ScatterLRooms", [typeof(CellRect), typeof(StructureLayout)]).Invoke(null, [size, layout]);
    }

    private static void ScatterSquareRooms(CellRect size, StructureLayout layout)
    {
        AccessTools.Method(typeof(LayoutWorkerLabyrinth), "ScatterSquareRooms", [typeof(CellRect), typeof(StructureLayout)]).Invoke(null, [size, layout]);
    }

    private static void CreateCorridorsAStar(StructureLayout layout)
    {
        AccessTools.Method(typeof(LayoutWorkerLabyrinth), "CreateCorridorsAStar", [typeof(StructureLayout)]).Invoke(null, [layout]);
    }

    private static void CreateDoors(StructureLayout layout)
    {
        AccessTools.Method(typeof(LayoutWorkerLabyrinth), "CreateDoors", [typeof(StructureLayout)]).Invoke(null, [layout]);
    }

    public static void FillEdges(CellRect rect, Sketch sketch)
    {
        AccessTools.Method(typeof(LayoutWorkerLabyrinth), "FillEdges", [typeof(CellRect), typeof(Sketch)]).Invoke(null, [rect, sketch]);
    }
}

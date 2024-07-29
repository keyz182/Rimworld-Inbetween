using System.Collections.Generic;
using System.Linq;
using LudeonTK;
using RimWorld;
using UnityEngine;
using Verse;

namespace Inbetween.MapGen.Labyrinth;

public class LabyrinthZoneMapComponent : CustomMapComponent
{
    public LabyrinthZoneMapComponent(Map map)
        : base(map)
    {
    }

    public List<LayoutRoom> SpawnableRooms;

    public void SetSpawnRooms(List<LayoutRoom> spawnableRooms)
    {
        SpawnableRooms = spawnableRooms;
    }

    public override void MapComponentOnGUI()
    {
        if (DebugViewSettings.drawMapGraphs)
        {
            foreach (KeyValuePair<Vector2, List<Vector2>> connection in map.layoutStructureSketch.structureLayout.neighbours.connections)
            {
                foreach (Vector2 vector2_ in connection.Value)
                {
                    Vector2 vector = new Vector2(2f, 2f);
                    Vector2 vector2_2 = vector + connection.Key;
                    Vector2 vector2_3 = vector + vector2_;
                    Vector2 vector2 = new Vector3(vector2_2.x, 0f, vector2_2.y).MapToUIPosition();
                    Vector2 vector3 = new Vector3(vector2_3.x, 0f, vector2_3.y).MapToUIPosition();
                    DevGUI.DrawLine(vector2, vector3, Color.green, 2f);
                }
            }
        }

        if (DebugViewSettings.drawMapRooms)
        {
            foreach (LayoutRoom room in map.layoutStructureSketch.structureLayout.Rooms)
            {
                string str = "NA";
                if (!room.defs.NullOrEmpty())
                {
                    str = room.defs.Select(x => x.defName).ToCommaList(false, false);
                }

                float widthCached = str.GetWidthCached();
                Vector2 uiPosition = (room.rects[0].Min + IntVec3.NorthEast * 2).ToVector3().MapToUIPosition();
                DevGUI.Label(new Rect(uiPosition.x - widthCached / 2f, uiPosition.y, widthCached, 20f), str);
                foreach (CellRect rect in room.rects)
                {
                    IntVec3 min = rect.Min;
                    IntVec3 intVec3_ = rect.Max + new IntVec3(1, 0, 1);
                    IntVec3 a = new IntVec3(min.x, 0, min.z);
                    IntVec3 intVec3_2 = new IntVec3(intVec3_.x, 0, min.z);
                    IntVec3 intVec3_3 = new IntVec3(min.x, 0, intVec3_.z);
                    IntVec3 b = new IntVec3(intVec3_.x, 0, intVec3_.z);
                    TryDrawLine(a, intVec3_2);
                    TryDrawLine(a, intVec3_3);
                    TryDrawLine(intVec3_3, b);
                    TryDrawLine(intVec3_2, b);
                }
            }
        }
    }

    private void TryDrawLine(IntVec3 a, IntVec3 b)
    {
        Vector2 vector = a.ToVector3().MapToUIPosition();
        Vector2 vector2 = b.ToVector3().MapToUIPosition();
        DevGUI.DrawLine(vector, vector2, Color.blue, 2f);
    }
}

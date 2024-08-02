using RimWorld;
using Verse;

namespace Inbetween.MapGen.Labyrinth;

public class RoomContentsDoor : RoomContentsWorker
{
    public override void FillRoom(Map map, LayoutRoom room)
    {
        IntVec3 cell;
        if (!room.TryGetRandomCellInRoom(map, out cell, 3))
        {
            return;
        }

        ModLog.Log("Placing Exit Door");
        GenSpawn.Spawn(ThingMaker.MakeThing(InbetweenDefOf.IB_Door, ThingDefOf.WoodLog), cell, map, Rot4.North);
    }
}

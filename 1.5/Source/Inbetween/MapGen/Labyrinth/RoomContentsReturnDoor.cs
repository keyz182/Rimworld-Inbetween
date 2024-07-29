using RimWorld;
using Verse;

namespace Inbetween.MapGen.Labyrinth;

public class RoomContentsReturnDoor : RoomContentsWorker
{
    public override void FillRoom(Map map, LayoutRoom room)
    {
        IntVec3 cell;
        if (!room.TryGetRandomCellInRoom(map, out cell, 3))
        {
            return;
        }

        Log.Message("Placing Entry Door");
        GenSpawn.Spawn(ThingMaker.MakeThing(InbetweenDefOf.IB_ReturnDoor, ThingDefOf.WoodLog), cell, map, Rot4.North);
    }
}

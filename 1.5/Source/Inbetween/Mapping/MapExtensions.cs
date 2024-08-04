using Inbetween.Buildings;
using Verse;

namespace Inbetween.Mapping;

public static class MapExtensions
{
    public static Building_InbetweenDoor StartDoor(this Map map)
    {
        return (Building_InbetweenDoor) map.listerThings.AllThings.FirstOrDefault(t => t is Building_InbetweenDoor);
    }

    public static Building_ReturnDoor ReturnDoor(this Map map)
    {
        return (Building_ReturnDoor) map.listerThings.AllThings.FirstOrDefault(t => t is Building_ReturnDoor);
    }
}

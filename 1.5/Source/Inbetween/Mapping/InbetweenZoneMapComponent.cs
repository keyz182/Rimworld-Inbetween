using System.Linq;
using Inbetween.MapGen;
using Verse;

namespace Inbetween.Mapping;

public class InbetweenZoneMapComponent : MapComponent
{
    public InbetweenZoneDef _inbetweenZoneDef;

    public InbetweenZoneMapComponent(Map map) : base(map)
    {
    }

    public override void MapComponentTick()
    {
        if (map.listerThings.ThingsOfDef(InbetweenDefOf.IB_Door).FirstOrDefault() is null)
        {
            GenStep_StartDoor gm = new GenStep_StartDoor();
            gm.def = InbetweenDefOf.IB_GenStep_InbetweenDoor;

            gm.Generate(map, new GenStepParams());
        }
    }
}

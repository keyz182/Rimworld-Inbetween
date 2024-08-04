using System;
using Inbetween.Mapping;
using RimWorld;
using Verse;

namespace Inbetween.Buildings;

public class IB_DoorExtension : DefModExtension
{
    public GraphicData extraGraphicData;
}

public class Building_IBPortal : MapPortal
{
    // Handle alt texture for door closed
    private IB_DoorExtension defExtension;

    // Helper to fetch the game component
    public InbetweenGameComponent IbGameComponent => Current.Game.GetComponent<InbetweenGameComponent>();

    public Graphic doorClosedGraphic;
    protected bool HasExtension => defExtension != null;

    public virtual bool ShouldUseAlternative()
    {
        return HasExtension && defExtension.extraGraphicData != null && !IbGameComponent.CanDoNextMap(this) && IsOpen();
    }

    public override Graphic Graphic => ShouldUseAlternative() ? AlternateGraphic : base.Graphic;

    protected Graphic AlternateGraphic
    {
        get
        {
            if (doorClosedGraphic != null)
            {
                return doorClosedGraphic;
            }

            if (!HasExtension || defExtension.extraGraphicData == null)
            {
                return BaseContent.BadGraphic;
            }

            doorClosedGraphic = defExtension.extraGraphicData.GraphicColoredFor(this);

            return doorClosedGraphic;
        }
    }


    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        defExtension = def.GetModExtension<IB_DoorExtension>();
    }

    public virtual string OpenCommandString => "IB_OpenPortal".Translate(Label);


    public InbetweenZoneMapComponent ZoneMapComponent => Map.GetComponent<InbetweenZoneMapComponent>();
    public Map LastMap => ZoneMapComponent.LastMap;
    public Map NextMap => ZoneMapComponent.NextMap;

    public virtual bool IsOpen()
    {
        return true;
    }

    public virtual void EnsureMap(Action callback)
    {
        throw new NotImplementedException();
    }

    public virtual void EnsureMap()
    {
        throw new NotImplementedException();
    }

    public override Map GetOtherMap()
    {
        throw new NotImplementedException();
    }

    public override IntVec3 GetDestinationLocation()
    {
        throw new NotImplementedException();
    }
}

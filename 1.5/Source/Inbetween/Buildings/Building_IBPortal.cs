using System;
using RimWorld;
using Verse;

namespace Inbetween.Buildings;

public class Building_IBPortal : MapPortal
{
    public virtual string OpenCommandString => "IB_OpenPortal".Translate(Label);

    public virtual bool IsOpen()
    {
        throw new NotImplementedException();
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

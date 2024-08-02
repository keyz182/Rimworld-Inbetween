using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;

namespace Inbetween.Buildings;

public class Building_ReturnDoor : Building_IBPortal
{
    public override string EnterCommandString => "Inbetween_EnterDoor".Translate();
    public override bool AutoDraftOnEnter => true;

    public Building_InbetweenDoor inbetweenDoor;


    public override bool IsOpen()
    {
        return true;
    }

    public override void EnsureMap(Action callback)
    {
        callback();
    }

    public override void EnsureMap()
    {
    }

    private static readonly CachedTexture ViewUndercaveTex = new CachedTexture("UI/Commands/ViewUndercave");

    public override Map GetOtherMap()
    {
        return inbetweenDoor.Map;
    }

    public override IntVec3 GetDestinationLocation()
    {
        return inbetweenDoor.Position;
    }

    public override bool IsEnterable(out string reason)
    {
        // return doors should always be enterable
        reason = "";
        return true;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref inbetweenDoor, "inbetweenDoor", false);
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (Gizmo g in base.GetGizmos())
            yield return g;

        if (inbetweenDoor == null)
        {
            yield break;
        }

        Command_Action gizmo = new Command_Action();
        gizmo.defaultLabel = "Inbetween_EnterDoor".Translate();
        gizmo.defaultDesc = "Inbetween_EnterDoor".Translate();
        gizmo.icon = ViewUndercaveTex.Texture;

        gizmo.action = () =>
        {
            CameraJumper.TryJumpAndSelect(inbetweenDoor, CameraJumper.MovementMode.Pan);
        };
        yield return gizmo;
    }

    public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
    {
        base.Destroy(mode);
        ModLog.Warn("Return door destroyed!");
        StackTrace t = new StackTrace();
        ModLog.Warn(t.ToStringSafe());
    }
}

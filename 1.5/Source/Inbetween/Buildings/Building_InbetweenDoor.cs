using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Inbetween.Mapping;
using Verse;

namespace Inbetween.Buildings;

public class Building_InbetweenDoor : Building_IBPortal
{
    public bool Open = false;

    public Building_ReturnDoor ReturnDoor => NextMap.ReturnDoor();

    // View next map button
    private static readonly CachedTexture ViewNextMapTex = new CachedTexture("UI/Commands/ViewUndercave");

    // Commands
    public override string EnterCommandString => "Inbetween_EnterDoor".Translate();
    public override bool AutoDraftOnEnter => true;


    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref Open, "Open");
    }

    public override bool ShouldUseAlternative()
    {
        return base.ShouldUseAlternative() && Open;
    }

    public override bool IsOpen()
    {
        return Open;
    }

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);

        if (map.IsPocketMap)
        {
            return;
        }

        if (Current.Game.GetComponent<InbetweenGameComponent>().RootDoor == this)
        {
            return;
        }

        if (Current.Game.GetComponent<InbetweenGameComponent>().RootDoor != null)
        {
            ModLog.Error("Cannot spawn multiple root doors");
            Destroy();
            return;
        }

        // We're on a non-pocket map, and there's no root door, so we're the root door
        Current.Game.GetComponent<InbetweenGameComponent>().RootDoor = this;
    }

    public override Map GetOtherMap()
    {
        return NextMap;
    }

    public override void EnsureMap(Action callback)
    {
        if (NextMap == null)
        {
            IbGameComponent.TryGenerateNextMap(this, callback);
        }
        else
        {
            callback?.Invoke();
        }
    }

    public override void EnsureMap()
    {
        EnsureMap(null);
    }

    public override IntVec3 GetDestinationLocation()
    {
        return ReturnDoor?.Position ?? IntVec3.Invalid;
    }

    public override bool IsEnterable(out string reason)
    {
        reason = "";
        if (GetOtherMap() == null)
        {
            reason = "IB_DoorNotOpen".Translate();
            return false;
        }

        if (!IbGameComponent.CanDoNextMap(this))
        {
            reason = "IB_PawnsWouldBeStuck".Translate();
            return false;
        }

        return true;
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (Gizmo gizmo in base.GetGizmos())
        {
            yield return gizmo;
        }

        if (NextMap != null)
        {
            Command_Action gizmo = new Command_Action();
            gizmo.defaultLabel = "Inbetween_EnterDoor".Translate();
            gizmo.defaultDesc = "Inbetween_EnterDoor".Translate();
            gizmo.icon = ViewNextMapTex.Texture;

            gizmo.action = () =>
            {
                CameraJumper.TryJumpAndSelect(ReturnDoor, CameraJumper.MovementMode.Pan);
            };
            yield return gizmo;
        }
    }

    public override string GetInspectString()
    {
        StringBuilder sb = new StringBuilder(IbGameComponent.CanDoNextMap(this) ? "IB_Open".Translate() : "IB_Closed".Translate());
        sb.Append(base.GetInspectString());
        return sb.ToString();
    }

    public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
    {
        base.Destroy(mode);
        ModLog.Warn("Return door destroyed!");
        StackTrace t = new StackTrace();
        ModLog.Warn(t.ToStringSafe());
    }
}

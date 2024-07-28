using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Inbetween.Mapping;

public class IB_DoorExtension : DefModExtension
{
    public GraphicData extraGraphicData;
}

public class Building_InbetweenDoor : MapPortal
{
    private IB_DoorExtension defExtension;

    public Building_ReturnDoor ReturnDoor;

    // The map this door will take you to
    public Map nextMap;


    // Handle alt texture for door closed
    private Graphic doorClosedGraphic;
    protected bool HasExtension => defExtension != null;
    public virtual bool ShouldUseAlternative => HasExtension && defExtension.extraGraphicData != null && !IbGameComponent.CanDoNextMap(this);
    public override Graphic Graphic => ShouldUseAlternative ? AlternateGraphic : base.Graphic;

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

    // View next map button
    private static readonly CachedTexture ViewNextMapTex = new CachedTexture("UI/Commands/ViewUndercave");

    // Commands
    public override string EnterCommandString => "Inbetween_EnterDoor".Translate();
    public override bool AutoDraftOnEnter => true;


    // Helper to fetch the game component
    public InbetweenGameComponent IbGameComponent => Current.Game.GetComponent<InbetweenGameComponent>();


    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        defExtension = def.GetModExtension<IB_DoorExtension>();

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
            Log.Error("Cannot spawn multiple root doors");
            Destroy();
            return;
        }

        // We're on a non-pocket map, and there's no root door, so we're the root door
        Current.Game.GetComponent<InbetweenGameComponent>().RootDoor = this;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref nextMap, "nextMap", false);
        Scribe_References.Look(ref ReturnDoor, "ReturnDoor", false);
    }

    public override Map GetOtherMap()
    {
        if (nextMap == null)
        {
            IbGameComponent.TryGenerateNextMap(this);
        }

        return nextMap;
    }

    public override IntVec3 GetDestinationLocation()
    {
        if (ReturnDoor != null)
        {
            return ReturnDoor.Position;
        }

        // try to generate a reference to the next map return door if we don't already have it
        if (nextMap?.listerThings.ThingsOfDef(InbetweenDefOf.IB_ReturnDoor).FirstOrDefault() is not Building_ReturnDoor rDoor)
        {
            return IntVec3.Invalid;
        }

        ReturnDoor = rDoor;
        rDoor.inbetweenDoor = this;

        return ReturnDoor.Position;
    }

    public override bool IsEnterable(out string reason)
    {
        reason = "";
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

        if (nextMap != null)
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
}

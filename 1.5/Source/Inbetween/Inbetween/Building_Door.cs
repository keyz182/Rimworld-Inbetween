using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Inbetween.Inbetween;

public class Building_InbetweenDoor: MapPortal
{
    public override string EnterCommandString => "Inbetween_EnterDoor".Translate();
    public override bool AutoDraftOnEnter => true;

    public Building_ReturnDoor ReturnDoor;

    private static readonly CachedTexture ViewUndercaveTex = new CachedTexture("UI/Commands/ViewUndercave");

    public Map undercave;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref undercave, "undercave", false);
        Scribe_References.Look(ref ReturnDoor, "ReturnDoor", false);
    }

    public override Map GetOtherMap()
    {
        if (undercave == null)
        {
            GenerateUndercave();
        }
        return undercave;
    }

    public override IntVec3 GetDestinationLocation()
    {
        if (ReturnDoor == null)
        {
            return IntVec3.Invalid;
        }
        return ReturnDoor.Position;
    }

    public void GenerateUndercave()
    {
        List<GenStepWithParams> extraSteps = new List<GenStepWithParams>();
        InbetweenGenDef newMapDef = Current.Game.GetComponent<InbetweenGameComponent>().NextMapGen();

        //Skip these if it's already defined in the mapgen
        if(!newMapDef.mapGenerator.genSteps.Any(s=>s==InbetweenDefOf.IB_GenStep_InbetweenDoor))
            extraSteps.Add(new GenStepWithParams(InbetweenDefOf.IB_GenStep_InbetweenDoor, new GenStepParams()));

        if(!newMapDef.mapGenerator.genSteps.Any(s=>s==InbetweenDefOf.IB_GenStep_InbetweenReturnDoor))
            extraSteps.Add(new GenStepWithParams(InbetweenDefOf.IB_GenStep_InbetweenReturnDoor, new GenStepParams()));

        // undercave = PocketMapUtility.GeneratePocketMap(new IntVec3(100, 1, 100), newMapDef.mapGenerator, extraSteps, Map);

        try
        {
            PocketMapParent parent = WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.PocketMap) as PocketMapParent;
            parent.sourceMap = Map;
            IntVec3 mapSize = new IntVec3(75, 1, 75);
            MapGeneratorDef gen = newMapDef.mapGenerator;

            undercave = MapGenerator.GenerateMap(mapSize, parent, gen, extraSteps, isPocketMap: true);
            Find.World.pocketMaps.Add(parent);

            InbetweenMapComponent ibmc = new InbetweenMapComponent(undercave);
            ibmc.InbetweenGenDef = newMapDef;
            undercave.components.Add(ibmc);

            ReturnDoor = undercave.listerThings.ThingsOfDef(InbetweenDefOf.IB_ReturnDoor).First() as Building_ReturnDoor;

            if (ReturnDoor != null)
            {
                ReturnDoor.inbetweenDoor = this;
            }
        }
        catch (Exception e)
        {
            Log.Error(e.ToString());
            throw;
        }
    }

    public override bool IsEnterable(out string reason)
    {
        reason = "";
        return true;
    }

    public override void OnEntered(Pawn pawn)
    {
        base.OnEntered(pawn);
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (Gizmo gizmo in base.GetGizmos())
            yield return gizmo;

        if (undercave != null)
        {
            Command_Action gizmo = new Command_Action();
            gizmo.defaultLabel = "Inbetween_EnterDoor".Translate();
            gizmo.defaultDesc = "Inbetween_EnterDoor".Translate();
            gizmo.icon = ViewUndercaveTex.Texture;

            gizmo.action = () =>
            {
                CameraJumper.TryJumpAndSelect(ReturnDoor, CameraJumper.MovementMode.Pan);
            };
            yield return gizmo;
        }
    }
}

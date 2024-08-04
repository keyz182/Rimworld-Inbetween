using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Inbetween.MapGen;
using RimWorld;
using Verse;

namespace Inbetween.Mapping;

public class InbetweenZoneMapComponent : MapComponent
{
    public InbetweenZoneDef InbetweenZoneDef;
    public Map LastMap;
    public Map NextMap;
    public bool Root = false;

    public virtual bool IsRootMap => Root;

    public Map GetLastMap()
    {
        return LastMap;
    }

    public Map GetNextMap()
    {
        return NextMap;
    }

    public InbetweenZoneMapComponent(Map map) : base(map)
    {
    }

    public InbetweenZoneMapComponent(Map map, Map lastMap, Map nextMap, List<IncidentDef> eventDefs) : base(map)
    {
        LastMap = lastMap;
        NextMap = nextMap;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Defs.Look(ref InbetweenZoneDef, "InbetweenZoneDef");
        Scribe_References.Look(ref LastMap, "LastMap");
        Scribe_References.Look(ref NextMap, "NextMap");
    }

    public override void MapComponentTick()
    {
        if (map.listerThings.ThingsOfDef(InbetweenDefOf.IB_Door).FirstOrDefault() is null)
        {
            GenStep_StartDoor gm = new GenStep_StartDoor();
            gm.def = InbetweenDefOf.IB_GenStep_InbetweenDoor;

            gm.Generate(map, new GenStepParams());
        }

        if (InbetweenZoneDef == null || !map.IsHashIntervalTick(600))
        {
            return;
        }

        ThreatsGeneratorParams tgParams = new ThreatsGeneratorParams
        {
            allowedThreats = AllowedThreatsGeneratorThreats.All,
            randSeed = Rand.Int,
            onDays = 1f,
            offDays = 0.5f,
            minSpacingDays = 0.04f,
            numIncidentsRange = new FloatRange(1f, 2f)
        };

        MethodInfo GetIncidentParms = AccessTools.Method(typeof(ThreatsGenerator), "GetIncidentParms");

        IncidentParms inParams = (IncidentParms) GetIncidentParms.Invoke(null, [tgParams, map]);

        ModLog.Log(inParams.ToString());
        ModLog.Log(InbetweenZoneDef.ToString());
        ModLog.Log(InbetweenZoneDef.eventDefs.ToString());

        IEnumerable<IncidentDef> eligableEvents = InbetweenZoneDef.eventDefs.Where(e => e.Worker.CanFireNow(inParams));

        foreach (IncidentDef eligableEvent in eligableEvents)
        {
            ModLog.Log(eligableEvent.ToString());

            // TODO : Fire events?
        }
    }
}

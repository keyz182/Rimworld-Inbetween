using System.Collections.Generic;
using Inbetween.Conditions;
using RimWorld;
using Verse;

namespace Inbetween.Mapping;

public class InbetweenZoneDef : Def
{
    // Our Zone definition
    public MapGeneratorDef mapGenerator;

    public List<IncidentDef> eventDefs;

    public List<DoorOpenConditionDef> doorOpenConditions;
}

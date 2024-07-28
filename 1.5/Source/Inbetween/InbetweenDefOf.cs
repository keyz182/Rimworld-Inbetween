using Inbetween.Inbetween;
using RimWorld;
using Verse;

namespace Inbetween;

[DefOf]
public static class InbetweenDefOf
{
    public static ScenarioDef IB_Quickstart;
    public static MapGeneratorDef IB_Empty;
    public static ThingDef IB_Door;
    public static ThingDef IB_ReturnDoor;
    public static GenStepDef IB_GenStep_InbetweenDoor;
    public static GenStepDef IB_GenStep_InbetweenReturnDoor;

    static InbetweenDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(InbetweenDefOf));
}

using RimWorld;
using Verse;

namespace Inbetween;

[DefOf]
public static class InbetweenDefOf
{
    // Remember to annotate any Defs that require a DLC as needed e.g.
    // [MayRequireBiotech]
    // public static GeneDef YourPrefix_YourGeneDefName;
    
    static InbetweenDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(InbetweenDefOf));
}

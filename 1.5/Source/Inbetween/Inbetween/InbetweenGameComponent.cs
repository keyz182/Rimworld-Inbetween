using RimWorld;
using Verse;

namespace Inbetween.Inbetween;

public class InbetweenGameComponent : GameComponent
{
    public bool InbetweenQuickplayMode = false;

    public InbetweenGameComponent()
    {

    }

    public InbetweenGameComponent(Game game)
    {

    }

    public InbetweenGenDef NextMapGen()
    {
        return DefDatabase<InbetweenGenDef>.GetRandom();
    }
}

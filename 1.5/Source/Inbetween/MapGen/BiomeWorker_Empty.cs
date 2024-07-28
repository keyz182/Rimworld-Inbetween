using RimWorld;
using RimWorld.Planet;

namespace Inbetween.MapGen;

public class BiomeWorker_Empty : BiomeWorker
{
    //
    public override float GetScore(Tile tile, int tileID)
    {
        return 0f;
    }
}

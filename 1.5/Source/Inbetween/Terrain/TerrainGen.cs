using Inbetween.Inbetween;

namespace Inbetween.Terrain;

public abstract class TerrainGen
{
    public InBetweenMap parent;
    public TerrainGenProperties props;

    public virtual void Initialize(TerrainGenProperties props) => this.props = props;

    public virtual void PostExposeData()
    {
    }
}

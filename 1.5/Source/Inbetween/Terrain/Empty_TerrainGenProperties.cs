namespace Inbetween.Terrain;

public class Empty_TerrainGenProperties : TerrainGenProperties
{
    public Empty_TerrainGenProperties() => this.terrainClass = typeof(EmptyTerrainGen);

}

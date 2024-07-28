using System.Collections.Generic;
using Verse;

namespace Inbetween.Terrain;

public class TerrainGenProperties
{
    [TranslationHandle]
    public System.Type terrainClass = typeof (TerrainGen);

    public TerrainGenProperties(){}

    public TerrainGenProperties(System.Type terrainClass) => this.terrainClass = terrainClass;


    public virtual IEnumerable<string> ConfigErrors(ThingDef parentDef)
    {
        if (this.terrainClass == (System.Type) null)
            yield return parentDef.defName + " has TerrainGenProperties with null terrainClass.";
    }
}

using System.Collections;
using UnityEngine;

/*
 * Sample implementation of the generator class
 */
public class TerrainGenerator : Generator
{
    public override void Clear()
    {
        Debug.Log("Clearing terrain");
    }

    public override IEnumerator Generate(WorldInfo worldInfo)
    {
        Debug.Log($"Generating {worldInfo.TerrainMaterial} terrain");
        yield return new WaitForSeconds(4);
    }
}
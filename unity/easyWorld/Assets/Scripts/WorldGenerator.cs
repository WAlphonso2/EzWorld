using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    public List<Generator> generators;

    public IEnumerator ClearCurrentWorld()
    {
        Debug.Log("Clearing current world");
        generators.ForEach(g => g.Clear());
        yield return null;
    }

    public void GenerateNewWorld(WorldInfo worldInfo)
    {
        /* 
         * tell all generators to generate their parts of the world which will run in parallel.
         * dependent generation should have a parent generator to contain needed logic.
         * eg. if your generator depends on terrain, put your generator inside of the terrain generator
         */
        foreach (Generator g in generators)
        {
            StartCoroutine(g.Generate(worldInfo));
        }

        Debug.Log("Started all generators successfully");
    }

    public void OnApplicationQuit()
    {
        StartCoroutine(ClearCurrentWorld());
    }
}

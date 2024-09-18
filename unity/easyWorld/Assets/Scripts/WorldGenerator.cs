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
        // Iterate over each terrain in worldInfo and generate its components
        for (int i = 0; i < worldInfo.terrainsData.Count; i++)
        {
            foreach (Generator g in generators)
            {
                StartCoroutine(g.Generate(worldInfo, i));  // Pass terrain index
            }
        }

        Debug.Log("Started all generators successfully");
    }

    public void OnApplicationQuit()
    {
        StartCoroutine(ClearCurrentWorld());
    }
}

﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    public List<Generator> generators;
    public CharacterSelectionManager characterSelectionManager;  

    public IEnumerator ClearCurrentWorld()
    {
        Debug.Log("Clearing current world");
        generators.ForEach(g => g.Clear());
        yield return null;
    }

    public void GenerateNewWorld(WorldInfo worldInfo)
    {
        // Generate the world and show character selection after terrain generation is complete
        StartCoroutine(GenerateWorldAndShowCharacterSelection(worldInfo));
    }

    private IEnumerator GenerateWorldAndShowCharacterSelection(WorldInfo worldInfo)
    {
        for (int i = 0; i < worldInfo.terrainsData.Count; i++)
        {
            foreach (Generator g in generators)
            {
                yield return StartCoroutine(g.Generate(worldInfo, i));
            }
        }

        Debug.Log("Terrain generation complete. Showing character selection UI.");

        // Pass the worldInfo to the CharacterSelectionManager
        characterSelectionManager.SetupWorldInfo(worldInfo, 0);

        // Show character selection UI after terrain generation is done
        characterSelectionManager.ShowCharacterSelection();
    }

    public void OnApplicationQuit()
    {
        StartCoroutine(ClearCurrentWorld());
    }
}

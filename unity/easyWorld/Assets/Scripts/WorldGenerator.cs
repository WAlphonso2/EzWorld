using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    // terrain dependent generators are run for each terrain index
    public List<Generator> terrainDependentGenerators;

    // terrain independent generators are run once
    public List<Generator> terrainIndependentGenerators;

    public CharacterSelectionManager characterSelectionManager;
    public bool VREnabled = false;
    public CharacterSelectionManager characterSelectionManager;
    public XROrigin vrCharacter;

    public IEnumerator ClearCurrentWorld()
    {
        Debug.Log("Clearing current world");
        terrainDependentGenerators.ForEach(g => g?.Clear());
        terrainIndependentGenerators.ForEach(g => g?.Clear());
        vrCharacter.gameObject.SetActive(false);
        yield return null;
    }

    public void GenerateNewWorld(WorldInfo worldInfo)
    {
        // Generate the world and show character selection after terrain generation is complete
        StartCoroutine(GenerateTerrainDependentWorld(worldInfo));
        StartCoroutine(GenerateTerrainIndependentWorld(worldInfo));
    }

    private IEnumerator GenerateTerrainDependentWorld(WorldInfo worldInfo)
    {
        for (int i = 0; i < worldInfo.terrainsData.Count; i++)
        {
            foreach (Generator g in terrainDependentGenerators)
            {
                yield return StartCoroutine(g.Generate(worldInfo, i));
            }
        }

        // show character after the terrain has been finished
        ShowCharacter(worldInfo);
    }

    private IEnumerator GenerateTerrainIndependentWorld(WorldInfo worldInfo)
    {
        terrainIndependentGenerators.ForEach(g => StartCoroutine(g.Generate(worldInfo)));
        yield return null;
    }

    private void ShowCharacter(WorldInfo worldInfo)
    {
        Debug.Log("Terrain generation complete. Showing character selection UI.");


        if (VREnabled)
        {
            Debug.Log("Terrain generation complete. Enabling VR character.");
            vrCharacter.gameObject.SetActive(true);
            vrCharacter.transform.position = new Vector3(512, 50, 512); // temp solution, need to do similar thing as placing normal character
        }
        else
        {
            Debug.Log("Terrain generation complete. Showing character selection UI.");
            // Pass the worldInfo to the CharacterSelectionManager
            characterSelectionManager.SetupWorldInfo(worldInfo, 0);

            // Show character selection UI after terrain generation is done
            characterSelectionManager.ShowCharacterSelection();
        }
    }

    public void OnApplicationQuit()
    {
        StartCoroutine(ClearCurrentWorld());
    }
}

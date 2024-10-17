using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    public bool VREnabled = false;
    public List<Generator> generators;
    public CharacterSelectionManager characterSelectionManager;
    public XROrigin vrCharacter;

    public IEnumerator ClearCurrentWorld()
    {
        Debug.Log("Clearing current world");
        generators.ForEach(g => g.Clear());
        vrCharacter.gameObject.SetActive(false);
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



        if (VREnabled)
        {
            Debug.Log("Terrain generation complete. Enabling VR character.");
            vrCharacter.gameObject.SetActive(true);
            vrCharacter.transform.position = new Vector3(512, 100, 512); // temp solution, need to do similar thing as placing normal character
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

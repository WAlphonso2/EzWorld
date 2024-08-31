using UnityEngine;
using System.Collections;


public class WorldBuilder : MonoBehaviour
{
    public AICommunicator aiCommunicator;
    public GameObject mountainPrefab;

    public string userDescription;


    void Start()
    {
        StartCoroutine(GenerateWorld(userDescription));
    }


    public IEnumerator GenerateWorld(string description)
    {
        yield return StartCoroutine(aiCommunicator.GetMapping(description, (responseText) =>
        {
            if (responseText != null)
            {
                ProcessAIOutput(responseText);
            }
            else
            {
                Debug.LogError("Failed to receive AI output.");
            }
        }));
    }


    private void ProcessAIOutput(string aiOutput)
    {        if (aiOutput.Contains("Terrain Type: mountain"))
        {
            CreateTerrain(mountainPrefab);
        }
        else
        {
            Debug.LogWarning("Unknown terrain type in AI output.");
        }
    }


    private void CreateTerrain(GameObject terrainPrefab)
    {
        // Instantiate the chosen terrain prefab in the scene
        GameObject terrain = Instantiate(terrainPrefab);
        terrain.transform.position = Vector3.zero; // Set position as needed
        Debug.Log(terrainPrefab.name + " terrain created.");
    }
}

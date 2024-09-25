using UnityEngine;
using System.Collections.Generic;

public class CharacterSelectionManager : MonoBehaviour
{
    public TerrainGenerator terrainGenerator;  // Reference to the TerrainGenerator script
    public GameObject scrollView;              // Reference to the Scroll View for character selection
    public GameObject[] characterPrefabs;      // All character prefabs
    public Camera initialCamera;               // The initial camera that shows before character selection
    private Camera activeCharacterCamera;      // The camera inside the selected character prefab
    public Transform spawnPoint;               // Spawn point for the selected character
    private GameObject currentCharacter;       // Reference to the currently selected character

    private WorldInfo worldInfo;
    private int terrainIndex;
    void Start()
    {
        activeCharacterCamera = null;
    }

    // Show the character selection UI after terrain generation
    public void ShowCharacterSelection()
    {
        scrollView.SetActive(true);
        Debug.Log("Character selection UI is now visible.");
    }


    // Call this method after terrain generation to store worldInfo and terrainIndex
    public void SetupWorldInfo(WorldInfo info, int index)
    {
        worldInfo = info;
        terrainIndex = index;
    }

    // Method called by the UI button
    public void SelectCharacter(int characterIndex)
    {
        Debug.Log("SelectCharacter called with characterIndex: " + characterIndex);

        // Instantiate the selected character
        SetSelectedCharacter(characterIndex);

        // Hide the character selection UI
        HideCharacterSelection();

        // Find and activate the camera inside the selected character prefab
        AssignCharacterCamera();

        // Position the player near the path after selecting the character
        PositionPlayerNearPath();
    }


    // Hide the character selection UI
    public void HideCharacterSelection()
    {
        scrollView.SetActive(false);
    }

    // Instantiate and set the selected character
    public void SetSelectedCharacter(int characterIndex)
    {
        // Destroy the previous character if it exists
        if (currentCharacter != null)
        {
            Destroy(currentCharacter);
        }

        // Instantiate the selected character at the spawn point
        currentCharacter = Instantiate(characterPrefabs[characterIndex], spawnPoint.position, spawnPoint.rotation);

        // Disable all other character controls
        DeactivateAllCharacters();

        // Enable control for the newly selected character
        EnableCharacterControl(currentCharacter);

        Debug.Log("Character selected: " + currentCharacter.name);
    }

    // Reassign the active camera to the one inside the selected character prefab
    private void AssignCharacterCamera()
    {
        if (currentCharacter == null)
        {
            Debug.LogError("Selected character not found.");
            return;
        }

        // Find the camera inside the selected character prefab
        activeCharacterCamera = currentCharacter.GetComponentInChildren<Camera>();
        if (activeCharacterCamera != null)
        {
            if (!activeCharacterCamera.enabled)
            {
                activeCharacterCamera.enabled = true;
            }
            if (!activeCharacterCamera.gameObject.activeSelf)
            {
                activeCharacterCamera.gameObject.SetActive(true);
            }

            initialCamera.gameObject.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            Debug.Log("Activated the camera inside the selected character prefab.");
        }
        else
        {
            Debug.LogError("No camera found in the selected character prefab.");
        }
    }

    // Disable all characters' movement components
    private void DeactivateAllCharacters()
    {
        foreach (var characterPrefab in characterPrefabs)
        {
            var characterInstance = GameObject.Find(characterPrefab.name);
            if (characterInstance != null)
            {
                DisableCharacterControl(characterInstance);
            }
        }
    }

    // Enable control on the selected character
    private void EnableCharacterControl(GameObject character)
    {
        var controller = character.GetComponent<CharacterController>();
        if (controller != null)
        {
            controller.enabled = true;
        }

        var inputScripts = character.GetComponentsInChildren<MonoBehaviour>();
        foreach (var script in inputScripts)
        {
            script.enabled = true;
        }
    }

    // Disable control on the non-selected characters
    private void DisableCharacterControl(GameObject character)
    {
        var controller = character.GetComponent<CharacterController>();
        if (controller != null)
        {
            controller.enabled = false;
        }

        var inputScripts = character.GetComponentsInChildren<MonoBehaviour>();
        foreach (var script in inputScripts)
        {
            script.enabled = false;
        }
    }

    // Position the player near a generated path
    private void PositionPlayerNearPath()
    {
        if (worldInfo == null)
        {
            Debug.LogError("WorldInfo not set.");
            return;
        }

        Terrain terrain = TerrainGenerator.GetTerrainByIndexOrCreate(terrainIndex, 0, 0, 0);
        if (terrain == null)
        {
            Debug.LogError("Failed to position player: Terrain not found.");
            return;
        }

        // Get a random position near the path
        Vector3 spawnPosition = GetRandomPositionNearPath(terrain);
        if (spawnPosition == Vector3.zero)
        {
            Debug.LogError("No valid path points found for player spawn.");
            return;
        }

        // Set the character's position near the path
        currentCharacter.transform.position = spawnPosition;

        Debug.Log("Player positioned at " + currentCharacter.transform.position);
    }


    // Get a random position near the path generated by PathGenerator
    private Vector3 GetRandomPositionNearPath(Terrain terrain)
    {
        // Simplified: fetch path points from TerrainGenerator's PathGenerator
        var pathPointsList = new List<Vector2Int>(terrainGenerator.pathGenerator.PathPoints);
        if (pathPointsList.Count == 0)
        {
            return Vector3.zero;
        }

        Vector2Int randomPathPoint = pathPointsList[Random.Range(0, pathPointsList.Count)];
        Vector3 worldPosition = new Vector3(randomPathPoint.x, 0, randomPathPoint.y);

        float yPos = GetHeightFromNoiseMap(randomPathPoint.x, randomPathPoint.y, terrain.terrainData.size.y);
        worldPosition.y = yPos;

        return worldPosition;
    }

    // Get the height from the noise map generated by HeightsGenerator
    private float GetHeightFromNoiseMap(int x, int z, float maxTerrainHeight)
    {
        if (worldInfo == null || worldInfo.heightMap == null)
        {
            Debug.LogError("Height map not available.");
            return 0f;
        }

        return worldInfo.heightMap[z, x] * maxTerrainHeight;
    }
}

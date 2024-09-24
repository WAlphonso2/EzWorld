using Assets.Scripts.MapGenerator.Generators;
using System.Collections.Generic; 
using System.Collections;
using UnityEngine;

public class TerrainGenerator : Generator
{
    public HeightsGenerator heightsGenerator;
    public TexturesGenerator texturesGenerator;
    public TreeGenerator treeGenerator;
    public GrassGenerator grassGenerator;
    public WaterGenerator waterGenerator;
    public PathGenerator pathGenerator;
    public RiverGenerator riverGenerator;

    public Camera initialCamera;
    public Camera thirdPersonCamera;
    public GameObject[] characterPrefabs;  
    public GameObject player;               
    public Transform spawnPoint;
    private GameObject currentCharacter;

    public override void Clear()
    {
        // Ensure that terrain exists before attempting to clear
        for (int i = 0; i < 10; i++) 
        {
            Terrain terrain = GameObject.Find($"Terrain_{i}")?.GetComponent<Terrain>();

            if (terrain != null)
            {
                heightsGenerator?.Clear();
                texturesGenerator?.Clear();
                treeGenerator?.Clear();
                grassGenerator?.Clear();
                waterGenerator?.Clear();
                pathGenerator?.Clear();
                riverGenerator?.Clear();

                Debug.Log($"Cleared terrain {i}");
            }
            else
            {
                Debug.LogWarning($"Terrain {i} not found, skipping clear.");
            }
        }
    }



    public override IEnumerator Generate(WorldInfo worldInfo, int terrainIndex)
    {
        // Generate terrain-specific data for the terrain index
        Terrain terrain = GetTerrainByIndexOrCreate(terrainIndex, worldInfo.terrainsData[terrainIndex].heightsGeneratorData.width,
                                                    worldInfo.terrainsData[terrainIndex].heightsGeneratorData.depth,
                                                    worldInfo.terrainsData[terrainIndex].heightsGeneratorData.height);

        if (terrain == null)
        {
            Debug.LogError($"Failed to create or retrieve terrain at index {terrainIndex}");
            yield break;
        }

        // Generate heights, textures, trees, grass, path, water
        yield return StartCoroutine(heightsGenerator.Generate(worldInfo, terrainIndex));
        yield return StartCoroutine(texturesGenerator.Generate(worldInfo, terrainIndex));
        yield return StartCoroutine(treeGenerator.Generate(worldInfo, terrainIndex));
        yield return StartCoroutine(grassGenerator.Generate(worldInfo, terrainIndex));
        yield return StartCoroutine(pathGenerator.Generate(worldInfo, terrainIndex));
        yield return StartCoroutine(waterGenerator.Generate(worldInfo, terrainIndex));

    }

    // Create or retrieve the terrain by index, ensuring it's positioned next to other terrains
    public static Terrain GetTerrainByIndexOrCreate(int terrainIndex, int width, int depth, int height)
    {
        // Find the terrain by name, or create it if it doesn't exist
        GameObject terrainGO = GameObject.Find($"Terrain_{terrainIndex}");
        Terrain terrain;

        if (terrainGO == null)
        {
            terrainGO = new GameObject($"Terrain_{terrainIndex}");
            terrain = terrainGO.AddComponent<Terrain>();
            TerrainCollider terrainCollider = terrainGO.AddComponent<TerrainCollider>();

            UnityEngine.TerrainData terrainData = new UnityEngine.TerrainData
            {
                heightmapResolution = width + 1,
                size = new Vector3(width, depth, height)
            };

            terrain.terrainData = terrainData;
            terrainCollider.terrainData = terrainData;

            // Position the terrain next to the previous one (assume each terrain is laid out horizontally)
            float terrainWidth = width;
            terrainGO.transform.position = new Vector3(terrainIndex * terrainWidth, 0, 0); // Adjust X position for each terrain

            Debug.Log($"Created new terrain at index {terrainIndex}");
        }
        else
        {
            terrain = terrainGO.GetComponent<Terrain>();
            Debug.Log($"Found existing terrain at index {terrainIndex}");
        }

        return terrain;
    }

    public static AnimationCurve GetHeightCurveFromType(string curveType)
    {
        switch (curveType.ToLower())
        {
            case "linear":
                return AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);

            case "constant":
                return AnimationCurve.Constant(0.0f, 1.0f, 1.0f);  // Constant value over time

            case "easein":
                return AnimationCurve.EaseInOut(0.0f, 0.0f, 1.0f, 1.0f);  // Ease-in (gradual start)

            case "easeout":
                return new AnimationCurve(new Keyframe(0.0f, 1.0f), new Keyframe(1.0f, 0.0f));  // Ease-out (gradual end)

            case "sine":
                return new AnimationCurve(
                    new Keyframe(0.0f, 0.0f),
                    new Keyframe(0.5f, 1.0f),
                    new Keyframe(1.0f, 0.0f));  // Simulate sine wave

            case "bezier":
                // Create a Bezier-like curve
                return new AnimationCurve(
                    new Keyframe(0.0f, 0.0f, 1.0f, 1.0f),
                    new Keyframe(0.5f, 1.0f, 0.0f, 0.0f),
                    new Keyframe(1.0f, 0.0f, -1.0f, -1.0f));

            default:
                Debug.LogWarning($"Unknown curve type: {curveType}, defaulting to linear.");
                return AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);  // Default to linear if unknown
        }
    }
  
    private void PositionPlayerNearPath(WorldInfo worldInfo, int terrainIndex)
    {
        // Get the terrain and make sure it's valid
        Terrain terrain = GetTerrainByIndexOrCreate(terrainIndex, 0, 0, 0);
        if (terrain == null)
        {
            Debug.LogError("Failed to position player: Terrain not found.");
            return;
        }

        // Get a random point near the path generated by PathGenerator
        Vector3 spawnPosition = GetRandomPositionNearPath(worldInfo, terrain);
        if (spawnPosition == Vector3.zero)
        {
            Debug.LogError("No valid path points found for player spawn.");
            return;
        }

        // Check if a character has been selected
        if (currentCharacter == null)
        {
            Debug.LogError("No character selected!");
            return;
        }

        // Set the character's position near the path
        currentCharacter.transform.position = spawnPosition;

        Debug.Log("Player positioned at " + currentCharacter.transform.position);
    }

    // Get a random position near the path generated by PathGenerator
    private Vector3 GetRandomPositionNearPath(WorldInfo worldInfo, Terrain terrain)
{
    // Convert the HashSet to a List so we can access elements by index
    List<Vector2Int> pathPointsList = new List<Vector2Int>(pathGenerator.PathPoints);

    if (pathPointsList == null || pathPointsList.Count == 0)
    {
        Debug.LogError("PathGenerator has no path points.");
        return Vector3.zero;
    }

    // Select a random point from the list of generated path points
    Vector2Int randomPathPoint = pathPointsList[Random.Range(0, pathPointsList.Count)];

    // Convert the 2D path point to a 3D world position
    Vector3 worldPosition = new Vector3(randomPathPoint.x, 0, randomPathPoint.y);

    // Get the height from the noise map generated by HeightsGenerator
    float yPos = GetHeightFromNoiseMap(worldInfo, randomPathPoint.x, randomPathPoint.y, terrain.terrainData.size.y);
    worldPosition.y = yPos;

    return worldPosition;
}


    // Get the height from the noise map generated by the HeightsGenerator
    private float GetHeightFromNoiseMap(WorldInfo worldInfo, int x, int z, float maxTerrainHeight)
    {
        // Ensure the height map exists in WorldInfo
        if (worldInfo.heightMap == null || x >= worldInfo.heightMap.GetLength(0) || z >= worldInfo.heightMap.GetLength(1))
        {
            Debug.LogError("Invalid height map or coordinates out of range.");
            return 0f;
        }

        // Return the height at the (x, z) position, scaled to the terrain's maximum height
        return worldInfo.heightMap[z, x] * maxTerrainHeight;
    }

    // This function is called to set the character that was selected in the UI
    public void SetSelectedCharacter(int characterIndex)
    {
        if (currentCharacter != null)
        {
            Destroy(currentCharacter); 
        }

        // Instantiate the selected character at the spawn point
        currentCharacter = Instantiate(characterPrefabs[characterIndex], spawnPoint.position, spawnPoint.rotation);
        Debug.Log("Character selected: " + currentCharacter.name);
    }

}

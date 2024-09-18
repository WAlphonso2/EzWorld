using Assets.Scripts.MapGenerator.Generators;
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
    public GameObject player;

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

        yield return StartCoroutine(SwitchToGamePlayMode(terrainIndex));
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
    private IEnumerator SwitchToGamePlayMode(int terrainIndex)
    {
        Debug.Log("Waiting for terrain generation to finish...");

        Debug.Log("Terrain generation complete. Switching cameras...");

        // Switch to third-person camera after terrain is generated
        initialCamera.enabled = false;
        thirdPersonCamera.enabled = true;
        thirdPersonCamera.gameObject.SetActive(true);

        Debug.Log("Camera switched. Positioning player...");
        PositionPlayerOnTerrain(terrainIndex);

        yield return null;
    }

    private void PositionPlayerOnTerrain(int terrainIndex)
    {
        Terrain terrain = GetTerrainByIndexOrCreate(terrainIndex, 0, 0, 0);  // Fetch the terrain
        if (terrain == null)
        {
            Debug.LogError("Failed to position player: Terrain not found.");
            return;
        }

        UnityEngine.TerrainData terrainData = terrain.terrainData;

        // Get the center point of the terrain
        float centerX = terrainData.size.x / 2;
        float centerZ = terrainData.size.z / 2;

        // Randomly position the player within a certain range around the center
        float range = Mathf.Min(terrainData.size.x, terrainData.size.z) * 0.25f;
        float randomX = Random.Range(centerX - range, centerX + range);
        float randomZ = Random.Range(centerZ - range, centerZ + range);

        // Get the height at the random position on the terrain
        float yPos = terrain.SampleHeight(new Vector3(randomX, 0, randomZ)) + 1f;

        // Position the player
        Vector3 playerPosition = new Vector3(randomX + (terrainIndex * terrainData.size.x), yPos, randomZ);
        player.transform.position = playerPosition;
        Debug.Log("Player positioned at " + player.transform.position);
    }
}

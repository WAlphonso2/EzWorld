using UnityEngine;
using System.Collections;
using CurvedPathGenerator;
using System.Collections.Generic;
using Assets.Scripts.MapGenerator.Generators;

public class WorldBuilder : MonoBehaviour
{
    public AICommunicator aiCommunicator;
    public string userDescription;

    private HeightsGenerator heightsGenerator;
    private TexturesGenerator texturesGenerator;
    private TreeGenerator treeGenerator;
    private GrassGenerator grassGenerator;
    private WaterGenerator waterGenerator;
    private PathGenerator pathGenerator;

    public Terrain terrain;  // Reference to the Terrain GameObject in the Inspector
    public Camera initialCamera;
    public Camera thirdPersonCamera;
    public GameObject player;

    void Start()
    {
        // Ensure AICommunicator is assigned
        if (aiCommunicator == null)
        {
            aiCommunicator = FindObjectOfType<AICommunicator>();
            if (aiCommunicator == null)
            {
                Debug.LogError("AICommunicator not found in the scene!");
                return;
            }
        }

        // Get the attached generators from the Terrain GameObject
        heightsGenerator = terrain.GetComponent<HeightsGenerator>();
        texturesGenerator = terrain.GetComponent<TexturesGenerator>();
        treeGenerator = terrain.GetComponent<TreeGenerator>();
        grassGenerator = terrain.GetComponent<GrassGenerator>();
        waterGenerator = terrain.GetComponent<WaterGenerator>();
        pathGenerator = terrain.GetComponent<PathGenerator>();
        
        if (heightsGenerator == null || texturesGenerator == null || treeGenerator == null || grassGenerator == null)
        {
            Debug.LogError("One or more generators are not found on the Terrain GameObject!");
            return;
        }

        // Ensure cameras and player are assigned
        if (initialCamera == null || thirdPersonCamera == null || player == null)
        {
            Debug.LogError("InitialCamera, ThirdPersonCamera, or Player not assigned!");
            return;
        }

        initialCamera.enabled = true;
        thirdPersonCamera.enabled = false;

        StartCoroutine(GenerateWorld(userDescription));
    }

    public IEnumerator GenerateWorld(string description)
    {
        if (string.IsNullOrEmpty(description))
        {
            Debug.LogWarning("User description is empty. Cannot generate world.");
            yield break;
        }

        // Fetch the AI-generated terrain data
        yield return StartCoroutine(aiCommunicator.GetMapping(description, (TerrainDataFromAI response) =>
        {
            if (response != null)
            {
                // Log the AI response to the console
                Debug.Log("AI Response: " + JsonUtility.ToJson(response));

                // Apply the terrain settings from AI data
                ApplyTerrainSettings(response);

                // Start generating the terrain using the generators
                GenerateTerrain();

                // Create the random path
                GenerateRandomPath();

                // Switch to gameplay mode after generation
                StartCoroutine(SwitchToGamePlayMode());
            }
            else
            {
                Debug.LogError("Invalid AI output or no terrain description provided.");
            }
        }));

    }

    private void ApplyTerrainSettings(TerrainDataFromAI data)
    {
        // Apply settings to the HeightsGenerator
        heightsGenerator.Width = data.heightsGenerator.width;
        heightsGenerator.Height = data.heightsGenerator.height;
        heightsGenerator.Depth = data.heightsGenerator.depth;
        heightsGenerator.Octaves = data.heightsGenerator.octaves;
        heightsGenerator.Scale = data.heightsGenerator.scale;
        heightsGenerator.Lacunarity = data.heightsGenerator.lacunarity;
        heightsGenerator.Persistance = data.heightsGenerator.persistence;
        heightsGenerator.Offset = data.heightsGenerator.heightCurveOffset;
        heightsGenerator.FalloffDirection = data.heightsGenerator.falloffDirection;
        heightsGenerator.FalloffRange = data.heightsGenerator.falloffRange;
        heightsGenerator.UseFalloffMap = data.heightsGenerator.useFalloffMap;
        heightsGenerator.Randomize = data.heightsGenerator.randomize;
        heightsGenerator.AutoUpdate = data.heightsGenerator.autoUpdate;
        heightsGenerator.HeightCurve = GetHeightCurveFromType(data.heightsGenerator.heightCurve);

        // Clear existing textures
        texturesGenerator.textures.Clear();

        // Loop through each texture and apply the respective settings
        foreach (var textureData in data.texturesGenerator)
        {
            Texture2D texture = Resources.Load<Texture2D>($"Textures/{textureData.texture}");
            if (texture != null)
            {
                _Texture newTexture = new _Texture
                {
                    Texture = texture,
                    Tilesize = new Vector2(textureData.tileSizeX, textureData.tileSizeY),
                    Type = 0, // Assuming height-based texture by default
                    HeightCurve = GetHeightCurveFromType(textureData.heightCurve)
                };
                texturesGenerator.textures.Add(newTexture);
            }
            else
            {
                Debug.LogError($"Texture '{textureData.texture}' not found in Resources/Textures folder.");
            }
        }

        // Apply settings to the WaterGenerator
        waterGenerator.waterLevel = data.waterGenerator.waterLevel;
        waterGenerator.riverWidthRange = data.waterGenerator.riverWidthRange;
        waterGenerator.randomize = data.waterGenerator.randomize;
        waterGenerator.autoUpdate = data.waterGenerator.autoUpdate;

        // You may also want to trigger water generation here:
        waterGenerator.GenerateWater(data.waterGenerator.waterType);

        // Apply tree and grass settings
        ApplyTreeSettings(data);
        ApplyGrassSettings(data);
    }



    private AnimationCurve GetHeightCurveFromType(string curveType)
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


    private void ApplyTreeSettings(TerrainDataFromAI data)
    {
        // Clear existing trees
        treeGenerator.TreePrototypes.Clear();

        if (data.treeGenerator.treePrototypes > 0)
        {
            for (int i = 0; i < data.treeGenerator.treePrototypes; i++)
            {
                GameObject treePrefab = Resources.Load<GameObject>($"Trees/treePrefab{i + 1}");
                if (treePrefab != null)
                {
                    treeGenerator.TreePrototypes.Add(treePrefab);
                }
                else
                {
                    Debug.LogError($"Tree prefab '{i + 1}' not found in Resources/Trees folder.");
                }
            }
        }
    }

    private void ApplyGrassSettings(TerrainDataFromAI data)
    {
        // Clear existing grass textures
        grassGenerator.GrassTextures.Clear();

        if (data.grassGenerator.grassTextures > 0)
        {
            for (int i = 0; i < data.grassGenerator.grassTextures; i++)
            {
                Texture2D grassTexture = Resources.Load<Texture2D>($"Grass/Grass {i + 1}");
                if (grassTexture != null)
                {
                    grassGenerator.GrassTextures.Add(grassTexture);
                }
                else
                {
                    Debug.LogError($"Grass texture '{i + 1}' not found in Resources/Grass folder.");
                }
            }
        }
    }


    // Generate terrain using the attached generators
    private void GenerateTerrain()
    {
        heightsGenerator.Generate();
        texturesGenerator.Generate();
        treeGenerator.Generate();
        grassGenerator.Generate();
        waterGenerator.Generate();

    }

    private void GenerateRandomPath()
    {
        // Set up the path properties
        pathGenerator.PathDensity = 10;  // Adjust path density
        pathGenerator.LineMehsWidth = 2.0f;  // Adjust path width
        pathGenerator.LineTiling = 20f;  // Adjust texture tiling
        pathGenerator.IsLivePath = true;
        pathGenerator.CreateMeshFlag = true;
        pathGenerator.IsClosed = false;
        pathGenerator.LineTexture = Resources.Load<Texture2D>("Textures/sand");  // Set the sand texture

        // Generate random nodes for the path
        pathGenerator.NodeList_World = GenerateRandomPathPoints();
        pathGenerator.AngleList_World = GenerateRandomPathAngles(pathGenerator.NodeList_World);

        // Update the path after generation
        pathGenerator.UpdatePath();
    }

    private List<Vector3> GenerateRandomPathPoints()
    {
        List<Vector3> points = new List<Vector3>();

        // Generate random path points within the terrain bounds
        for (int i = 0; i < 5; i++)  // Example: create 5 random nodes
        {
            float x = Random.Range(0, terrain.terrainData.size.x);
            float z = Random.Range(0, terrain.terrainData.size.z);
            float y = terrain.SampleHeight(new Vector3(x, 0, z));

            points.Add(new Vector3(x, y + 1f, z));  // Add a slight offset in height
        }

        return points;
    }

    private List<Vector3> GenerateRandomPathAngles(List<Vector3> nodeList)
    {
        List<Vector3> angles = new List<Vector3>();

        // Generate random angles for each node (for the curve between nodes)
        for (int i = 0; i < nodeList.Count - 1; i++)
        {
            Vector3 midPoint = (nodeList[i] + nodeList[i + 1]) / 2;
            float x = midPoint.x + Random.Range(-5, 5);  // Add some randomness to the angle
            float z = midPoint.z + Random.Range(-5, 5);
            float y = terrain.SampleHeight(new Vector3(x, 0, z));

            angles.Add(new Vector3(x, y, z));
        }

        return angles;
    }


    private IEnumerator SwitchToGamePlayMode()
    {
        Debug.Log("Waiting for terrain generation to finish...");

        // Wait until terrain is generated
        while (!TerrainIsGenerated())
        {
            yield return null;  // Wait for the next frame
        }

        Debug.Log("Terrain generation complete. Switching cameras...");

        // Switch to third-person camera after terrain is generated
        initialCamera.enabled = false;
        thirdPersonCamera.enabled = true;
        thirdPersonCamera.gameObject.SetActive(true);

        Debug.Log("Camera switched. Positioning player...");
        PositionPlayerOnTerrain();
    }


    private bool TerrainIsGenerated()
    {
        // Check if the terrain data exists
        Terrain terrain = Terrain.activeTerrain;
        if (terrain == null)
        {
            Debug.LogError("No active terrain found.");
            return false;
        }

        // Check if the terrain data heightmap is properly populated
        TerrainData terrainData = terrain.terrainData;
        if (terrainData == null || terrainData.heightmapResolution == 0)
        {
            Debug.LogError("Terrain data is missing or not initialized.");
            return false;
        }

        // Check if terrain has a TerrainCollider
        TerrainCollider terrainCollider = terrain.GetComponent<TerrainCollider>();
        if (terrainCollider == null || terrainCollider.terrainData == null)
        {
            Debug.LogError("TerrainCollider is not properly set up.");
            return false;
        }

        // If all checks pass, assume terrain generation is complete
        Debug.Log("Terrain generation is complete.");
        return true;
    }


    private void PositionPlayerOnTerrain()
    {
        TerrainData terrainData = terrain.terrainData;

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
        Vector3 playerPosition = new Vector3(randomX, yPos, randomZ);
        player.transform.position = playerPosition;
        Debug.Log("Player positioned at " + player.transform.position);
    }


}


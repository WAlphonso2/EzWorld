using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.MapGenerator.Generators;

public class RiverGenerator : Generator
{
    public Terrain terrain;
    public int riverWidth = 4;  // Width of the river
    public float riverDepth = 10f;  // Depth of the river
    public int curveSmoothness = 200;  // Number of points to smooth the river
    public int maxRiverSegments = 85;  // Maximum number of river segments
    public float minSegmentLength = 70f;  // Minimum length of each river segment
    public float maxSegmentLength = 120f;  // Maximum length of each river segment
    public float slopeFactor = 0.9f;  // Factor controlling slope of river depth
    public bool allowSplit = true;  // Enable or disable splitting
    public float splitSpread = 25;  // How much the split paths should spread out
    public GameObject[] rockPrefabs;  
    public int rockSpacing = 10;  
    public Texture2D riversideTexture;
    public List<Vector2> mainRiverPathPoints; 
    private List<Vector2> splitRiverPathPoints;
    public WaterGenerator waterGenerator;

    public override IEnumerator Generate(WorldInfo worldInfo, int terrainIndex)
    {

        terrain = TerrainGenerator.GetTerrainByIndexOrCreate(terrainIndex, worldInfo.terrainsData[terrainIndex].heightsGeneratorData.width, 
                                                            worldInfo.terrainsData[terrainIndex].heightsGeneratorData.depth, 
                                                            worldInfo.terrainsData[terrainIndex].heightsGeneratorData.height);
                                                                        
        if (terrain == null)
        {
            Debug.LogError($"No terrain found or created for index {terrainIndex}");
            yield break;
        }

        Debug.Log($"Generating river on terrain {terrainIndex}");

        UnityEngine.TerrainData terrainData = terrain.terrainData;
        int terrainWidth = terrainData.alphamapWidth;
        int terrainLength = terrainData.alphamapHeight;

        // Generate the main river path
        mainRiverPathPoints = GenerateSmoothSnakeRiverPath(terrainWidth, terrainLength);

        // Optionally split the river midway
        if (allowSplit)
        {
            splitRiverPathPoints = GenerateSplitRiverPath(mainRiverPathPoints, terrainWidth, terrainLength);
        }

        List<Vector2> smoothMainRiverPath = SmoothBezierPath(mainRiverPathPoints, curveSmoothness);
        List<Vector2> smoothSplitRiverPath = allowSplit ? SmoothBezierPath(splitRiverPathPoints, curveSmoothness) : null;

        CarveShallowRiverPath(terrainData, smoothMainRiverPath, waterGenerator.waterLevel);
        if (allowSplit && smoothSplitRiverPath != null)
        {
            CarveShallowRiverPath(terrainData, smoothSplitRiverPath, waterGenerator.waterLevel);
        }

        PlaceRocksAlongRiver(smoothMainRiverPath);
        if (allowSplit && smoothSplitRiverPath != null)
        {
            PlaceRocksAlongRiver(smoothSplitRiverPath);
        }

        ApplySandTextureAlongRiver(terrainData, smoothMainRiverPath);
        if (allowSplit && smoothSplitRiverPath != null)
        {
            ApplySandTextureAlongRiver(terrainData, smoothSplitRiverPath);
        }

        List<Vector2> combinedRiverPath = new List<Vector2>(smoothMainRiverPath);
        if (allowSplit && smoothSplitRiverPath != null)
        {
            combinedRiverPath.AddRange(smoothSplitRiverPath);
        }

        waterGenerator.FillRiverWithWaterSingleObject(terrain, terrainData, combinedRiverPath);

        Debug.Log($"River generation completed for Terrain {terrainIndex}");

        yield return null;
    }


	private List<Vector2> GenerateSplitRiverPath(List<Vector2> mainPath, int terrainWidth, int terrainLength)
    {
        List<Vector2> splitPath = new List<Vector2>();

        // Find the midpoint of the main river path for the split
        int midpointIndex = mainPath.Count / 2;
        Vector2 splitStartPoint = mainPath[midpointIndex];
        splitPath.Add(splitStartPoint);

        // Generate a split that spreads away from the main path
        float currentX = splitStartPoint.x;
        float currentZ = splitStartPoint.y;

        for (int i = 0; i < maxRiverSegments / 2; i++)  // Limit the split to half of the main river segments
        {
            float segmentLength = Random.Range(minSegmentLength, maxSegmentLength);
            currentX = Mathf.Clamp(currentX + Random.Range(-segmentLength, segmentLength), 0, terrainWidth - 1);
            currentZ = Mathf.Clamp(currentZ + Random.Range(-segmentLength, segmentLength), 0, terrainLength - 1);

            splitPath.Add(new Vector2(currentX, currentZ));

            // Stop early if we've reached the edge of the terrain
            if (currentX >= terrainWidth - 1 || currentZ >= terrainLength - 1)
            {
                break;
            }
        }

        return splitPath;
    }

private List<Vector2> GenerateSmoothSnakeRiverPath(int terrainWidth, int terrainLength)
{
    List<Vector2> riverPath = new List<Vector2>();

    // Start the river at the left edge of the terrain (x = 0) or right edge (x = terrainWidth - 1)
    float startX = (Random.value > 0.5f) ? 0 : terrainWidth - 1;
    float startZ = Random.Range(terrainLength * 0.3f, terrainLength * 0.7f);  // Randomly choose a starting point along the Z axis near the middle

    riverPath.Add(new Vector2(startX, startZ));

    float currentX = startX;
    float currentZ = startZ;
    float lastDirection = (currentX == 0) ? 1 : -1;  // Move toward the opposite edge
    float forwardDirection = (startX == 0) ? 1 : -1;  // Force consistent forward movement in X-axis

    // Define a height threshold to avoid high areas (mountains)
    float heightThreshold = 0.6f * terrain.terrainData.size.y;  // Adjust this as needed

    bool isCurving = false;
    for (int i = 0; i < maxRiverSegments; i++)
    {
        float segmentLength = Random.Range(minSegmentLength, maxSegmentLength);

        // Always move forward in the X-axis, towards the opposite side of the terrain
        currentX = Mathf.Clamp(currentX + segmentLength * forwardDirection, 0, terrainWidth - 1);

        // Introduce a curve with slight Z-axis variation for snake-like movement
        if (Random.value > 0.7f && !isCurving)  // Start a curve with a small probability
        {
            isCurving = true;
            currentZ = Mathf.Clamp(currentZ + Random.Range(segmentLength * 0.2f, segmentLength * 0.4f), 0, terrainLength - 1);  // Slight Z-axis curve
        }
        else if (isCurving)  // Return to the main path after curving
        {
            isCurving = false;
            currentZ = Mathf.Clamp(currentZ - Random.Range(segmentLength * 0.2f, segmentLength * 0.4f), 0, terrainLength - 1);  // Reverse the Z-axis curve
        }

        // Check the height at the current point and adjust to avoid mountains
        float terrainHeightAtPoint = terrain.SampleHeight(new Vector3(currentX, 0, currentZ));

        if (terrainHeightAtPoint > heightThreshold)
        {
            // Adjust the Z to avoid high terrain (move downward)
            currentZ = Mathf.Clamp(currentZ - segmentLength * 0.5f, 0, terrainLength - 1);

            // If still too high, move slightly sideways
            if (terrain.SampleHeight(new Vector3(currentX, 0, currentZ)) > heightThreshold)
            {
                currentX = Mathf.Clamp(currentX - segmentLength * forwardDirection * 0.5f, 0, terrainWidth - 1);
            }
        }

        // Ensure the river continues forward, adjusting X but not going backward
        if ((forwardDirection == 1 && currentX < riverPath[riverPath.Count - 1].x) ||
            (forwardDirection == -1 && currentX > riverPath[riverPath.Count - 1].x))
        {
            currentX = Mathf.Clamp(riverPath[riverPath.Count - 1].x + segmentLength * forwardDirection, 0, terrainWidth - 1);
        }

        riverPath.Add(new Vector2(currentX, currentZ));

        // Stop early if we've reached the opposite edge of the terrain
        if (currentX == 0 || currentX == terrainWidth - 1)
        {
            break;
        }
    }

    // Ensure the river ends at the opposite edge of the terrain
    currentX = (forwardDirection == 1) ? terrainWidth - 1 : 0;
    riverPath.Add(new Vector2(currentX, currentZ));  // Force the final point to the edge

    return riverPath;
}



private void CarveShallowRiverPath(UnityEngine.TerrainData terrainData, List<Vector2> smoothRiverPath, float waterLevel)
{
    // Get the current heightmap data
    float[,] heights = terrainData.GetHeights(0, 0, terrainData.heightmapResolution, terrainData.heightmapResolution);

    int terrainWidth = terrainData.heightmapResolution;
    int terrainLength = terrainData.heightmapResolution;

    // Define the total width of the river and shore
    int shorelineWidth = riverWidth * 2;  // Shoreline extends on both sides of the river

    // Adjust the heights along the river path to create the riverbed and shorelines
    foreach (Vector2 point in smoothRiverPath)
    {
        int x = Mathf.RoundToInt(point.x);
        int z = Mathf.RoundToInt(point.y);

        // Loop over the area around the river to include the riverbed and shorelines
        for (int i = -shorelineWidth / 2; i < shorelineWidth / 2; i++)
        {
            for (int j = -shorelineWidth / 2; j < shorelineWidth / 2; j++)
            {
                int newX = Mathf.Clamp(x + i, 0, terrainWidth - 1);
                int newZ = Mathf.Clamp(z + j, 0, terrainLength - 1);

                float distanceFromCenter = Vector2.Distance(new Vector2(newX, newZ), new Vector2(x, z));

                // Inside the riverbed (steep slope)
                if (distanceFromCenter <= riverWidth / 2f)
                {
                    // Lower the terrain for the riverbed using a smoother adjustment
                    float riverbedHeightAdjustment = Mathf.SmoothStep(0, riverDepth / terrainData.size.y, distanceFromCenter / (riverWidth / 2f));

                    // Apply smoothing by blending with the original height
                    heights[newZ, newX] = Mathf.Lerp(heights[newZ, newX], heights[newZ, newX] - riverbedHeightAdjustment * slopeFactor, 0.5f);
                }
                // Shoreline area (gentle slope)
                else if (distanceFromCenter <= shorelineWidth / 2f)
                {
                    // Create a smoother slope for the shorelines using SmoothStep
                    float shoreSlope = Mathf.SmoothStep(0, riverDepth / terrainData.size.y, (distanceFromCenter - riverWidth / 2f) / ((shorelineWidth / 2f) - (riverWidth / 2f)));

                    // Apply a smoother transition to the shoreline
                    heights[newZ, newX] = Mathf.Lerp(heights[newZ, newX], heights[newZ, newX] - shoreSlope * slopeFactor * 0.5f, 0.5f);
                }

                // Ensure terrain height matches water height at river edges
                if (distanceFromCenter > riverWidth / 2f && distanceFromCenter <= shorelineWidth / 2f)
                {
                    // Set the terrain height exactly at the water level to connect terrain and water
                    float adjustedWaterHeight = Mathf.Lerp(heights[newZ, newX], (waterLevel / terrainData.size.y), 0.7f); // Adjust slope connection
                    heights[newZ, newX] = adjustedWaterHeight;
                }
            }
        }
    }

    // Apply the modified heights back to the terrain
    terrainData.SetHeights(0, 0, heights);

    // Apply a smoothing filter to remove spikes
    ApplyTerrainSmoothing(terrainData, smoothRiverPath, shorelineWidth);
}



private void ApplyTerrainSmoothing(TerrainData terrainData, List<Vector2> smoothRiverPath, int smoothingRadius)
{
    float[,] heights = terrainData.GetHeights(0, 0, terrainData.heightmapResolution, terrainData.heightmapResolution);
    int terrainWidth = terrainData.heightmapResolution;
    int terrainLength = terrainData.heightmapResolution;

    foreach (Vector2 point in smoothRiverPath)
    {
        int x = Mathf.RoundToInt(point.x);
        int z = Mathf.RoundToInt(point.y);

        for (int i = -smoothingRadius; i <= smoothingRadius; i++)
        {
            for (int j = -smoothingRadius; j <= smoothingRadius; j++)
            {
                int newX = Mathf.Clamp(x + i, 0, terrainWidth - 1);
                int newZ = Mathf.Clamp(z + j, 0, terrainLength - 1);

                // Average the height values of surrounding points to smooth the terrain
                float totalHeight = 0f;
                int count = 0;
                for (int k = -1; k <= 1; k++)
                {
                    for (int l = -1; l <= 1; l++)
                    {
                        int neighborX = Mathf.Clamp(newX + k, 0, terrainWidth - 1);
                        int neighborZ = Mathf.Clamp(newZ + l, 0, terrainLength - 1);
                        totalHeight += heights[neighborZ, neighborX];
                        count++;
                    }
                }

                heights[newZ, newX] = totalHeight / count;  // Set the current point to the average height
            }
        }
    }

    // Apply the smoothed heights back to the terrain
    terrainData.SetHeights(0, 0, heights);
}




    // Smooth the path using Bezier curve interpolation
    private List<Vector2> SmoothBezierPath(List<Vector2> pathPoints, int smoothness)
    {
        List<Vector2> smoothPath = new List<Vector2>();

        // For each segment, create a Bezier curve between two points
        for (int i = 0; i < pathPoints.Count - 2; i += 2)
        {
            Vector2 p0 = pathPoints[i];
            Vector2 p1 = pathPoints[i + 1];
            Vector2 p2 = pathPoints[i + 2];

            for (int t = 0; t <= smoothness; t++)
            {
                float u = t / (float)smoothness;
                Vector2 point = (1 - u) * (1 - u) * p0 + 2 * (1 - u) * u * p1 + u * u * p2; // Quadratic Bezier formula
                smoothPath.Add(point);
            }
        }

        return smoothPath;
    }

    public override void Clear()
    {
        if (terrain == null)
        {
            Debug.LogError("No terrain assigned.");
            return;
        }

        Debug.Log("Clearing river data");

        UnityEngine.TerrainData terrainData = terrain.terrainData;

        int terrainWidth = terrainData.alphamapWidth;
        int terrainLength = terrainData.alphamapHeight;

        float[,,] originalAlphamaps = new float[terrainLength, terrainWidth, terrainData.alphamapLayers];

        // Reset to default (assuming first texture is the base texture)
        for (int z = 0; z < terrainLength; z++)
        {
            for (int x = 0; x < terrainWidth; x++)
            {
                for (int layer = 0; layer < terrainData.alphamapLayers; layer++)
                {
                    originalAlphamaps[z, x, layer] = (layer == 0) ? 1 : 0;
                }
            }
        }

        terrainData.SetAlphamaps(0, 0, originalAlphamaps);

        Debug.Log("River data cleared.");
    }

       // Place rocks along the left and right edges of the river
    private void PlaceRocksAlongRiver(List<Vector2> smoothRiverPath)
    {
        if (rockPrefabs.Length == 0)
        {
            Debug.LogWarning("No rock prefabs assigned.");
            return;
        }

        // Loop through the river path and place rocks at regular intervals along the left and right banks
        for (int i = 0; i < smoothRiverPath.Count; i += rockSpacing)
        {
            Vector2 point = smoothRiverPath[i];
            float terrainHeight = terrain.SampleHeight(new Vector3(point.x, 0, point.y));

            // Calculate positions for the left and right bank rocks
            Vector3 leftBank = new Vector3(point.x - riverWidth * 0.6f, terrainHeight, point.y);
            Vector3 rightBank = new Vector3(point.x + riverWidth * 0.6f, terrainHeight, point.y);

            // Randomly choose a rock prefab and instantiate it
            GameObject rockLeft = Instantiate(rockPrefabs[Random.Range(0, rockPrefabs.Length)], leftBank, RandomRotation());
            GameObject rockRight = Instantiate(rockPrefabs[Random.Range(0, rockPrefabs.Length)], rightBank, RandomRotation());

            // Slightly adjust the X and Z positions for variation
            rockLeft.transform.position += new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
            rockRight.transform.position += new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
        }
    }

    // Generate a random rotation for rocks
    private Quaternion RandomRotation()
    {
        return Quaternion.Euler(0, Random.Range(0, 360f), 0);  // Rotate only along the Y-axis for natural placement
    }

    private void ApplySandTextureAlongRiver(TerrainData terrainData, List<Vector2> smoothRiverPath)
    {

        // Disable this method in play mode
        if (Application.isPlaying)
        {
            Debug.LogWarning("ApplySandTextureAlongRiver is disabled in play mode.");
            return;
        }
        
        int terrainWidth = terrainData.alphamapWidth;
        int terrainLength = terrainData.alphamapHeight;

        // Get the existing alphamaps
        float[,,] alphaMap = terrainData.GetAlphamaps(0, 0, terrainWidth, terrainLength);

        // Load the riverside texture from Resources
        Texture2D riversideTexture = Resources.Load<Texture2D>("Textures/riverside");
        if (riversideTexture == null)
        {
            Debug.LogError("Failed to load riverside texture.");
            return;
        }

        // Check if the riverside texture is already in the terrain's layers
        int riversideTextureIndex = GetTextureIndex(terrainData, riversideTexture);
        if (riversideTextureIndex == -1)
        {
            // If the texture isn't found, create a new TerrainLayer and add it to the terrain
            TerrainLayer newLayer = new TerrainLayer();
            newLayer.diffuseTexture = riversideTexture;
            newLayer.tileSize = new Vector2(10, 10);  // Adjust the tiling size as needed

            // Add the new layer to the terrain
            List<TerrainLayer> terrainLayers = new List<TerrainLayer>(terrainData.terrainLayers);
            terrainLayers.Add(newLayer);
            terrainData.terrainLayers = terrainLayers.ToArray();

            // Update the texture index after adding the new layer
            riversideTextureIndex = terrainLayers.Count - 1;
            Debug.Log("Added new riverside texture to terrain.");
        }

        // Define the width of the sandy riverbank
        int shorelineWidth = riverWidth * 3;

        // Loop through the river path and apply the riverside texture near the edges
        foreach (Vector2 point in smoothRiverPath)
        {
            int centerX = Mathf.RoundToInt(point.x / terrainData.size.x * terrainWidth);
            int centerZ = Mathf.RoundToInt(point.y / terrainData.size.z * terrainLength);

            // Ensure that centerX and centerZ are within bounds
            centerX = Mathf.Clamp(centerX, 0, terrainWidth - 1);
            centerZ = Mathf.Clamp(centerZ, 0, terrainLength - 1);

            for (int x = Mathf.Max(0, centerX - shorelineWidth); x < Mathf.Min(terrainWidth, centerX + shorelineWidth); x++)
            {
                for (int z = Mathf.Max(0, centerZ - shorelineWidth); z < Mathf.Min(terrainLength, centerZ + shorelineWidth); z++)
                {
                    if (x >= 0 && x < terrainWidth && z >= 0 && z < terrainLength)
                    {
                        float distanceToCenter = Vector2.Distance(new Vector2(x, z), new Vector2(centerX, centerZ));

                        if (distanceToCenter <= shorelineWidth && distanceToCenter >= riverWidth)
                        {
                            float t = Mathf.InverseLerp(riverWidth, shorelineWidth, distanceToCenter);

                            // Apply riverside texture blend
                            alphaMap[z, x, riversideTextureIndex] = Mathf.Lerp(0, 1, t);

                            // Set other textures to blend accordingly
                            for (int i = 0; i < terrainData.alphamapLayers; i++)
                            {
                                if (i != riversideTextureIndex)
                                {
                                    alphaMap[z, x, i] = 1 - alphaMap[z, x, riversideTextureIndex];
                                }
                            }
                        }
                    }
                }
            }
        }

        // Apply the updated alphamaps to the terrain
        terrainData.SetAlphamaps(0, 0, alphaMap);
    }


    private int GetTextureIndex(TerrainData terrainData, Texture2D texture)
    {
        for (int i = 0; i < terrainData.terrainLayers.Length; i++)
        {
            if (terrainData.terrainLayers[i].diffuseTexture == texture)
            {
                return i;
            }
        }
        return -1;
    }
}

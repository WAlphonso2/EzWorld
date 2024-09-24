using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class PathGenerator : Generator
{
    public Terrain terrain; 
    public int pathWidth = 10;
    public Texture2D selectedTexture;
    public int curveSmoothness = 75;
    public int splitChance = 20;
    public float splitSpread = 30f;
    public HashSet<Vector2Int> PathPoints { get; private set; } = new HashSet<Vector2Int>();

    public override IEnumerator Generate(WorldInfo worldInfo, int terrainIndex)
    {
        // Use GetTerrainByIndexOrCreate to ensure the terrain exists
        terrain = TerrainGenerator.GetTerrainByIndexOrCreate(terrainIndex, worldInfo.terrainsData[terrainIndex].heightsGeneratorData.width, 
                                                            worldInfo.terrainsData[terrainIndex].heightsGeneratorData.depth, 
                                                            worldInfo.terrainsData[terrainIndex].heightsGeneratorData.height);
        if (terrain == null)
        {
            Debug.LogError($"No terrain found or created for index {terrainIndex}");
            yield break;
        }

        if (selectedTexture == null)
        {
            Debug.LogError("No texture selected for the path.");
            yield break;
        }

        Debug.Log($"Generating paths on terrain {terrainIndex}");

        UnityEngine.TerrainData terrainData = terrain.terrainData;
        int pathTextureIndex = GetTextureIndexOrAdd(terrain, selectedTexture);

        if (pathTextureIndex == -1)
        {
            Debug.LogError("Selected texture not found in terrain textures.");
            yield break;
        }

        int terrainWidth = terrainData.alphamapWidth;
        int terrainLength = terrainData.alphamapHeight;
        List<List<Vector2>> xDirectionPaths = GenerateSplitPathsInXDirection(terrainWidth, terrainLength);
        List<List<Vector2>> zDirectionPaths = GenerateSplitPathsInZDirection(terrainWidth, terrainLength);
        List<Vector2> smoothPathPoints = new List<Vector2>();

        foreach (var path in xDirectionPaths)
        {
            smoothPathPoints.AddRange(SmoothBezierPath(path, curveSmoothness));
        }
        foreach (var path in zDirectionPaths)
        {
            smoothPathPoints.AddRange(SmoothBezierPath(path, curveSmoothness));
        }

        float[,,] alphamaps = terrainData.GetAlphamaps(0, 0, terrainWidth, terrainLength);
        foreach (Vector2 point in smoothPathPoints)
        {
            int x = Mathf.RoundToInt(point.x);
            int z = Mathf.RoundToInt(point.y);

            for (int i = -pathWidth / 2; i < pathWidth / 2; i++)
            {
                for (int j = -pathWidth / 2; j < pathWidth / 2; j++)
                {
                    int newX = Mathf.Clamp(x + i, 0, terrainWidth - 1);
                    int newZ = Mathf.Clamp(z + j, 0, terrainLength - 1);

                    for (int layer = 0; layer < alphamaps.GetLength(2); layer++)
                    {
                        if (layer == pathTextureIndex)
                        {
                            alphamaps[newZ, newX, layer] = 1;
                        }
                        else
                        {
                            alphamaps[newZ, newX, layer] = 0;
                        }
                    }

                    PathPoints.Add(new Vector2Int(newX, newZ)); 
                }
            }
        }
        terrainData.SetAlphamaps(0, 0, alphamaps);
        Debug.Log($"Path generation completed for Terrain {terrainIndex}");

        // Call ClearVegetation after path generation
        ClearVegetationOnPath(terrainData);

        yield return null;
    }

    private void ClearVegetationOnPath(UnityEngine.TerrainData terrainData)
    {
        // Scale factors for converting world position (PathPoints) to detail layer resolution
        float detailResolutionScaleX = terrainData.detailWidth / (float)terrainData.alphamapWidth;
        float detailResolutionScaleY = terrainData.detailHeight / (float)terrainData.alphamapHeight;

        // Iterate over all detail layers (grass)
        for (int layer = 0; layer < terrainData.detailPrototypes.Length; layer++)
        {
            // Get the current detail layer data (grass layer)
            int[,] detailLayer = terrainData.GetDetailLayer(0, 0, terrainData.detailWidth, terrainData.detailHeight, layer);

            // Iterate over each path point and clear grass from the path
            foreach (Vector2Int pathPoint in PathPoints)
            {
                // Scale path point to detail resolution
                int detailX = Mathf.RoundToInt(pathPoint.x * detailResolutionScaleX);
                int detailY = Mathf.RoundToInt(pathPoint.y * detailResolutionScaleY);

                // Ensure the points are within the detail layer bounds
                if (detailX >= 0 && detailX < terrainData.detailWidth && detailY >= 0 && detailY < terrainData.detailHeight)
                {
                    // Clear grass (set detail to 0) at this point
                    detailLayer[detailY, detailX] = 0;
                }
            }

            // Set the modified detail layer back to the terrain data
            terrainData.SetDetailLayer(0, 0, layer, detailLayer); 
        }

        // Remove trees from tree instances
        List<TreeInstance> remainingTrees = new List<TreeInstance>();
        float treeClearDistance = (pathWidth * 2)- 1; 

        foreach (var tree in terrainData.treeInstances)
        {
            // Convert tree position to Vector2Int
            Vector2Int treePosition = new Vector2Int((int)(tree.position.x * terrainData.size.x), (int)(tree.position.z * terrainData.size.z));

            bool isFarFromPath = true;

            // Check if the tree is within the "clear zone" around the path
            foreach (Vector2Int pathPoint in PathPoints)
            {
                float distanceToPath = Vector2Int.Distance(treePosition, pathPoint);
                
                // If the tree is within the clear distance, mark it to be removed
                if (distanceToPath <= treeClearDistance)
                {
                    isFarFromPath = false;
                    break;
                }
            }

            // If the tree is far enough from the path, keep it
            if (isFarFromPath)
            {
                remainingTrees.Add(tree);
            }
        }

        terrainData.treeInstances = remainingTrees.ToArray();

        Debug.Log("Vegetation (grass and trees) on path cleared.");
    }





    // Generate random points for the path that starts from the left (X-axis movement)
    private List<List<Vector2>> GenerateSplitPathsInXDirection(int terrainWidth, int terrainLength)
    {
        List<List<Vector2>> splitPaths = new List<List<Vector2>>();
        List<Vector2> mainPath = new List<Vector2>();

        Vector2 startPoint = new Vector2(0, Random.Range(terrainLength / 4, terrainLength * 3 / 4)); // Start randomly along the Z-axis
        mainPath.Add(startPoint);

        // Generate points along the X axis with random splits, extending to the edge
        for (int x = 10; x < terrainWidth; x += Random.Range(20, 40)) // Adjust spacing
        {
            float randomZ = Random.Range(terrainLength / 4, terrainLength * 3 / 4); // Move along Z-axis
            mainPath.Add(new Vector2(x, randomZ));

            // Random chance to split the path
            if (Random.Range(0, 100) < splitChance)
            {
                List<Vector2> splitPath = new List<Vector2>(mainPath); // Start the split path from the main path
                for (int sx = x; sx < terrainWidth; sx += Random.Range(20, 40))
                {
                    // Spread out the split path more as it progresses
                    float spread = (sx - x) * splitSpread / terrainWidth; // Adjust spread
                    float splitZ = Mathf.Clamp(randomZ + Random.Range(-spread, spread), 0, terrainLength); // Move along Z-axis with spread
                    splitPath.Add(new Vector2(sx, splitZ));
                }
                splitPaths.Add(splitPath);
            }
        }

        // Ensure main path reaches the end of the X axis
        mainPath.Add(new Vector2(terrainWidth - 1, Random.Range(terrainLength / 4, terrainLength * 3 / 4)));
        splitPaths.Insert(0, mainPath);

        return splitPaths;
    }

    // Generate random points for the path that starts from the bottom (Z-axis movement)
    private List<List<Vector2>> GenerateSplitPathsInZDirection(int terrainWidth, int terrainLength)
    {
        List<List<Vector2>> splitPaths = new List<List<Vector2>>();
        List<Vector2> mainPath = new List<Vector2>();

        Vector2 startPoint = new Vector2(Random.Range(terrainWidth / 4, terrainWidth * 3 / 4), 0); // Start randomly along the X-axis
        mainPath.Add(startPoint);

        // Generate points along the Z axis with random splits, extending to the edge
        for (int z = 10; z < terrainLength; z += Random.Range(20, 40)) // Adjust spacing
        {
            float randomX = Random.Range(terrainWidth / 4, terrainWidth * 3 / 4); // Move along X-axis
            mainPath.Add(new Vector2(randomX, z));

            // Random chance to split the path
            if (Random.Range(0, 100) < splitChance)
            {
                List<Vector2> splitPath = new List<Vector2>(mainPath); // Start the split path from the main path
                for (int sz = z; sz < terrainLength; sz += Random.Range(20, 40))
                {
                    // Spread out the split path more as it progresses
                    float spread = (sz - z) * splitSpread / terrainLength; // Adjust spread
                    float splitX = Mathf.Clamp(randomX + Random.Range(-spread, spread), 0, terrainWidth); // Move along X-axis with spread
                    splitPath.Add(new Vector2(splitX, sz));
                }
                splitPaths.Add(splitPath);
            }
        }

        // Ensure main path reaches the end of the Z axis
        mainPath.Add(new Vector2(Random.Range(terrainWidth / 4, terrainWidth * 3 / 4), terrainLength - 1));
        splitPaths.Insert(0, mainPath);

        return splitPaths;
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

    // Helper function to find the texture index in the terrain's splatmap or add it if not found
    private int GetTextureIndexOrAdd(Terrain terrain, Texture2D targetTexture)
    {
        TerrainLayer[] terrainLayers = terrain.terrainData.terrainLayers;

        // Check the terrain layers for the correct texture
        for (int i = 0; i < terrainLayers.Length; i++)
        {
            if (terrainLayers[i] != null && terrainLayers[i].diffuseTexture == targetTexture)
            {
                return i;  // Return the correct index in the terrain layers
            }
        }

        // If the texture is not found, add it as a new layer
        Debug.Log("Adding new texture to terrain layers.");
        TerrainLayer newLayer = new TerrainLayer();
        newLayer.diffuseTexture = targetTexture;
        newLayer.tileSize = new Vector2(10, 10);  // Set the default tile size (adjust as necessary)

        List<TerrainLayer> layerList = new List<TerrainLayer>(terrainLayers);
        layerList.Add(newLayer);

        terrain.terrainData.terrainLayers = layerList.ToArray();  // Update the terrain layers

        // Return the index of the newly added texture
        return layerList.Count - 1;
    }

    public override void Clear()
    {
        if (terrain == null)
        {
            Debug.LogError("No terrain assigned.");
            return;
        }

        Debug.Log("Clearing path data");

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

        Debug.Log("Path data cleared.");
    }
}

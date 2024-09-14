using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class PathGenerator : Generator
{
    public Terrain terrain; // Drag and drop the terrain in the Inspector
    public int pathWidth = 10;  // Width of the path
    public Texture2D selectedTexture; // Texture selected from the editor
    public int curveSmoothness = 75; // Number of points to smooth the path
    public int splitChance = 20; // Percentage chance to split the path at each point
    public float splitSpread = 30f; // How much the split paths should spread out

    public override IEnumerator Generate(WorldInfo worldInfo)
    {
        if (terrain == null)
        {
            Debug.LogError("No terrain assigned.");
            yield break;
        }

        if (selectedTexture == null)
        {
            Debug.LogError("No texture selected for the path.");
            yield break;
        }

        Debug.Log("Generating paths on terrain");

        UnityEngine.TerrainData terrainData = terrain.terrainData;

        int pathTextureIndex = GetTextureIndexOrAdd(terrain, selectedTexture);

        if (pathTextureIndex == -1)
        {
            Debug.LogError("Selected texture not found in terrain textures.");
            yield break;
        }

        int terrainWidth = terrainData.alphamapWidth;
        int terrainLength = terrainData.alphamapHeight; // Z-axis for length of the terrain

        // Generate the first path moving in X direction
        List<List<Vector2>> xDirectionPaths = GenerateSplitPathsInXDirection(terrainWidth, terrainLength);

        // Generate the second path moving in Z direction
        List<List<Vector2>> zDirectionPaths = GenerateSplitPathsInZDirection(terrainWidth, terrainLength);

        // Combine both sets of paths
        List<Vector2> smoothPathPoints = new List<Vector2>();
        foreach (var path in xDirectionPaths)
        {
            smoothPathPoints.AddRange(SmoothBezierPath(path, curveSmoothness));
        }
        foreach (var path in zDirectionPaths)
        {
            smoothPathPoints.AddRange(SmoothBezierPath(path, curveSmoothness));
        }

        // Get the existing alphamap
        float[,,] alphamaps = terrainData.GetAlphamaps(0, 0, terrainWidth, terrainLength);

        // Apply the paths on the terrain
        foreach (Vector2 point in smoothPathPoints)
        {
            int x = Mathf.RoundToInt(point.x);
            int z = Mathf.RoundToInt(point.y); // Z-axis movement

            // Draw the path on the alphamap
            for (int i = -pathWidth / 2; i < pathWidth / 2; i++)
            {
                for (int j = -pathWidth / 2; j < pathWidth / 2; j++)
                {
                    int newX = Mathf.Clamp(x + i, 0, terrainWidth - 1);
                    int newZ = Mathf.Clamp(z + j, 0, terrainLength - 1); // Z-axis clamp

                    for (int layer = 0; layer < alphamaps.GetLength(2); layer++)
                    {
                        if (layer == pathTextureIndex)
                        {
                            alphamaps[newZ, newX, layer] = 1; // Set the selected texture for the path
                        }
                        else
                        {
                            alphamaps[newZ, newX, layer] = 0; // Clear other textures
                        }
                    }
                }
            }
        }

        // Apply the modified alphamap back to the terrain
        terrainData.SetAlphamaps(0, 0, alphamaps);

        Debug.Log("Path generation completed");
        yield return null;
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

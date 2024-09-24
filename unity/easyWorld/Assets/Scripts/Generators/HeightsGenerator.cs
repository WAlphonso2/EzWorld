using Assets.Scripts.MapGenerator.Maps;
using System.Collections;
using UnityEngine;

public class HeightsGenerator : Generator
{
    public int Width = 256;
    public int Height = 256;
    public int Depth = 10;
    public int Octaves = 4;
    public float Scale = 50f;
    public float Lacunarity = 2f;
    [Range(0, 1)]
    public float Persistance = 0.5f;
    public AnimationCurve HeightCurve;
    public float Offset = 100f;
    public float FalloffDirection = 3f;
    public float FalloffRange = 3f;
    public bool UseFalloffMap;
    public bool Randomize;
    public bool AutoUpdate;

    public Material URPTerrainMaterial; // Add this to assign a URP material

    private void OnValidate()
    {
        if (Width < 1) Width = 1;
        if (Height < 1) Height = 1;
        if (Lacunarity < 1) Lacunarity = 1;
        if (Octaves < 0) Octaves = 0;
        if (Scale <= 0) Scale = 0.0001f;
    }

    public override IEnumerator Generate(WorldInfo worldInfo, int terrainIndex)
    {
        // Fetch terrain-specific data from worldInfo
        CustomTerrainData terrainData = worldInfo.terrainsData[terrainIndex];
        LoadSettings(terrainData.heightsGeneratorData);  // Load the specific settings for this terrain

        // Use the GetTerrainByIndexOrCreate method from TerrainGenerator
        Terrain terrain = TerrainGenerator.GetTerrainByIndexOrCreate(terrainIndex, terrainData.heightsGeneratorData.width, terrainData.heightsGeneratorData.depth, terrainData.heightsGeneratorData.height);
        if (terrain == null)
        {
            Debug.LogError($"No terrain found or created for index {terrainIndex}");
            yield break;
        }

        UnityEngine.TerrainData terrainUnityData = terrain.terrainData;

        // Set up terrain size and resolution based on the data
        terrainUnityData.heightmapResolution = terrainData.heightsGeneratorData.width + 1;
        terrainUnityData.alphamapResolution = terrainData.heightsGeneratorData.width;
        terrainUnityData.SetDetailResolution(terrainData.heightsGeneratorData.width, 8);
        terrainUnityData.size = new Vector3(terrainData.heightsGeneratorData.width, terrainData.heightsGeneratorData.depth, terrainData.heightsGeneratorData.height);

        // Assign URP-compatible terrain material if using URP
        if (URPTerrainMaterial != null)
        {
            terrain.materialTemplate = URPTerrainMaterial;
        }
        else
        {
            Debug.LogWarning("URP Terrain Material not assigned.");
        }

        // Generate falloff map if needed
        float[,] falloff = null;
        if (UseFalloffMap)
        {
            falloff = new FalloffMap
            {
                FalloffDirection = FalloffDirection,
                FalloffRange = FalloffRange,
                Size = terrainData.heightsGeneratorData.width
            }.Generate();
        }

        // Generate the noise map
        float[,] noiseMap = GenerateNoise(falloff);
        terrainUnityData.SetHeights(0, 0, noiseMap);

        // Store the noiseMap in WorldInfo for further use
        worldInfo.heightMap = noiseMap;

        // Debugging: log the height map values for the first few points
        Debug.Log($"Height map stored for Terrain {terrainIndex}. First 10 height values:");
        for (int y = 0; y < Mathf.Min(terrainData.heightsGeneratorData.height, 10); y++)
        {
            for (int x = 0; x < Mathf.Min(terrainData.heightsGeneratorData.width, 10); x++)
            {
                Debug.Log($"Height at [{x},{y}]: {worldInfo.heightMap[y, x]}");
            }
        }

        yield return null;
    }

    // Generates noise for the terrain
    float[,] GenerateNoise(float[,] falloffMap = null)
    {
        AnimationCurve heightCurve = new AnimationCurve(HeightCurve.keys);

        float maxLocalNoiseHeight;
        float minLocalNoiseHeight;

        float[,] noiseMap = new PerlinMap()
        {
            Size = Width,
            Octaves = Octaves,
            Scale = Scale,
            Offset = Offset,
            Persistance = Persistance,
            Lacunarity = Lacunarity
        }.Generate(out maxLocalNoiseHeight, out minLocalNoiseHeight);

        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                var lerp = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[x, y]);

                if (falloffMap != null)
                {
                    lerp -= falloffMap[x, y];
                }

                if (lerp >= 0)
                {
                    noiseMap[x, y] = heightCurve.Evaluate(lerp);
                }
                else
                {
                    noiseMap[x, y] = 0;
                }
            }
        }

        return noiseMap;
    }

    public override void Clear()
    {
        if (Terrain.activeTerrain != null && Terrain.activeTerrain.terrainData != null)
        {
            UnityEngine.TerrainData terrainData = Terrain.activeTerrain.terrainData;
            terrainData.SetHeights(0, 0, new float[Width, Height]);
            Debug.Log("Heights cleared.");
        }
        else
        {
            Debug.LogWarning("No active terrain found to clear heights.");
        }
    }


    // Load the height generator settings from data
    private void LoadSettings(HeightsGeneratorData data)
    {
        if (data == null)
        {
            Debug.Log("HeightsGeneratorData is null");
            return;
        }

        Width = data.width;
        Height = data.height;
        Depth = data.depth;
        Octaves = data.octaves;
        Scale = data.scale;
        Lacunarity = data.lacunarity;
        Persistance = data.persistence;
        Offset = data.heightCurveOffset;
        FalloffDirection = data.falloffDirection;
        FalloffRange = data.falloffRange;
        UseFalloffMap = data.useFalloffMap;
        Randomize = data.randomize;
        AutoUpdate = data.autoUpdate;
        HeightCurve = TerrainGenerator.GetHeightCurveFromType(data.heightCurve);
    }
}

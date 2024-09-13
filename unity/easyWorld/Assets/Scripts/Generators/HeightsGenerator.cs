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

    // New shallow depth field
    public float ShallowDepth = 1f;

    private void OnValidate()
    {
        if (Width < 1)
        {
            Width = 1;
        }
        if (Height < 1)
        {
            Height = 1;
        }
        if (Lacunarity < 1)
        {
            Lacunarity = 1;
        }
        if (Octaves < 0)
        {
            Octaves = 0;
        }
        if (Scale <= 0)
        {
            Scale = 0.0001f;
        }
    }

    public override IEnumerator Generate(WorldInfo worldInfo)
    {

        // Check if there's an active terrain
        Terrain terrain = Terrain.activeTerrain;
        if (terrain == null)
        {
            Debug.LogError("No active terrain found. Add a terrain to the scene.");
            yield break;
        }
        
        LoadSettings(worldInfo.terrainData.heightsGeneratorData);

        if (Randomize)
        {
            Offset = Random.Range(0f, 9999f);
        }

        UnityEngine.TerrainData terrainData = Terrain.activeTerrain.terrainData;

        terrainData.heightmapResolution = Width + 1;
        terrainData.alphamapResolution = Width;
        terrainData.SetDetailResolution(Width, 8);

        terrainData.size = new Vector3(Width, Depth, Height);

        float[,] falloff = null;
        if (UseFalloffMap)
        {
            falloff = new FalloffMap
            {
                FalloffDirection = FalloffDirection,
                FalloffRange = FalloffRange,
                Size = Width
            }.Generate();
        }

        float[,] noiseMap = GenerateNoise(falloff);
        terrainData.SetHeights(0, 0, noiseMap);

        // Store the noiseMap in WorldInfo
        worldInfo.heightMap = noiseMap;

        // Log height map values for debugging
        Debug.Log("Height map stored in WorldInfo. First 10 height values:");
        for (int y = 0; y < Mathf.Min(Height, 10); y++)
        {
            for (int x = 0; x < Mathf.Min(Width, 10); x++)
            {
                Debug.Log($"Height at [{x},{y}]: {worldInfo.heightMap[y, x]}");
            }
        }

        yield return null;
    }

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
        UnityEngine.TerrainData terrainData = Terrain.activeTerrain.terrainData;
        terrainData.SetHeights(0, 0, new float[Width, Height]);
    }

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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.MapGenerator.Maps;

namespace Assets.Scripts.MapGenerator.Generators
{
    public class TreeGenerator : Generator
    {
        public int Octaves = 4;
        public float Scale = 40;
        public float Lacunarity = 2f;
        [Range(0, 1)]
        public float Persistence = 0.5f;
        public float Offset = 100f;
        public float MinLevel = 0;
        public float MaxLevel = 100;
        [Range(0, 90)]
        public float MaxSteepness = 70;
        [Range(-1, 1)]
        public float IslandSize = 0;
        [Range(0, 1)]
        public float Density = 0.5f;
        public bool Randomize;
        public bool AutoUpdate;

        public List<GameObject> TreePrototypes;

    public override IEnumerator Generate(WorldInfo worldInfo, int terrainIndex)
    {
        // Load specific tree generation settings for this terrain
        LoadSettings(worldInfo.terrainsData[terrainIndex].treeGeneratorData);

        if (Randomize)
        {
            Offset = Random.Range(0f, 9999f);
        }

        // Get the correct terrain based on the index or create it if it doesn't exist
        Terrain terrain = TerrainGenerator.GetTerrainByIndexOrCreate(terrainIndex, worldInfo.terrainsData[terrainIndex].heightsGeneratorData.width, worldInfo.terrainsData[terrainIndex].heightsGeneratorData.depth, worldInfo.terrainsData[terrainIndex].heightsGeneratorData.height);
        
        if (terrain == null)
        {
            Debug.LogError("No terrain found for the given terrain index.");
            yield break;
        }

        UnityEngine.TerrainData terrainData = terrain.terrainData;

        // Create tree prototypes
        List<TreePrototype> treePrototypes = new List<TreePrototype>();
        foreach (var t in TreePrototypes)
        {
            treePrototypes.Add(new TreePrototype() { prefab = t });
        }

        terrainData.treePrototypes = treePrototypes.ToArray();

        // Clear any existing tree instances
        terrainData.treeInstances = new TreeInstance[0];

        // Generate positions for new trees
        List<Vector3> treePos = new List<Vector3>();

        float maxLocalNoiseHeight;
        float minLocalNoiseHeight;

        float[,] noiseMap = new PerlinMap()
        {
            Size = terrainData.alphamapWidth,
            Octaves = Octaves,
            Scale = Scale,
            Offset = Offset,
            Persistance = Persistence,
            Lacunarity = Lacunarity
        }.Generate(out maxLocalNoiseHeight, out minLocalNoiseHeight);

        // Tree counter
        int treeCount = 0;
        int maxTreeLimit = 500;

        // Iterate over terrain points to place trees
        for (int x = 0; x < terrainData.alphamapWidth; x++)
        {
            for (int y = 0; y < terrainData.alphamapHeight; y++)
            {
                if (treeCount >= maxTreeLimit)
                {
                    // Exit loop if we have reached the maximum number of trees
                    break;
                }

                float height = terrainData.GetHeight(x, y);
                float heightScaled = height / terrainData.size.y;
                float xScaled = (x + Random.Range(-1f, 1f)) / terrainData.alphamapWidth;
                float yScaled = (y + Random.Range(-1f, 1f)) / terrainData.alphamapHeight;
                float steepness = terrainData.GetSteepness(xScaled, yScaled);

                float noiseStep = Random.Range(0f, 1f);
                float noiseVal = noiseMap[x, y];

                // Check conditions for placing trees
                if (
                    noiseStep < Density &&
                    noiseVal < IslandSize &&
                    steepness < MaxSteepness &&
                    height > MinLevel &&
                    height < MaxLevel
                )
                {
                    treePos.Add(new Vector3(xScaled, heightScaled, yScaled));
                    treeCount++; // Increment tree counter
                }
            }
        }

        // Create TreeInstance array from generated tree positions
        TreeInstance[] treeInstances = new TreeInstance[treePos.Count];

        for (int i = 0; i < treeInstances.Length; i++)
        {
            treeInstances[i].position = treePos[i];
            treeInstances[i].prototypeIndex = Random.Range(0, treePrototypes.Count);
            treeInstances[i].color = new Color(Random.Range(0.4f, 1f), Random.Range(0.4f, 1f), Random.Range(0.4f, 1f));
            treeInstances[i].lightmapColor = Color.white;
            treeInstances[i].heightScale = 1.0f + Random.Range(-0.25f, 0.5f);
            treeInstances[i].widthScale = 1.0f + Random.Range(-0.5f, 0.25f);
        }

        terrainData.treeInstances = treeInstances;

        Debug.Log($"{treeInstances.Length} trees were created on terrain {terrainIndex}");

        yield return null;
    }
public List<GameObject> TreePrototypes2;

        public override void Clear()
        {
            // Check if there is an active terrain
            if (Terrain.activeTerrain != null)
            {
                // Check if the terrain has valid terrainData
                if (Terrain.activeTerrain.terrainData != null)
                {
                    // Clear all tree instances from the terrain
                    Terrain.activeTerrain.terrainData.treeInstances = new TreeInstance[0];
                    Debug.Log("Trees cleared.");
                }
                else
                {
                    Debug.LogWarning("Active terrain has no valid terrain data to clear trees.");
                }
            }
            else
            {
                Debug.LogWarning("No active terrain found to clear trees.");
            }
        }


        private void LoadSettings(TreeGeneratorData data)
        {
            if (data == null)
            {
                Debug.LogError("TreeGeneratorData is null");
                return;
            }

            Octaves = data.octaves;
            Scale = data.scale;
            Lacunarity = data.lacunarity;
            Persistence = data.persistence;
            Offset = data.offset;
            MinLevel = data.minLevel;
            MaxLevel = data.maxLevel;
            MaxSteepness = data.maxSteepness;
            IslandSize = data.islandSize;
            Density = data.density;
            Randomize = data.randomize;

            TreePrototypes.Clear();

            if (data.treePrototypes > 0)
            {
                for (int i = 0; i < data.treePrototypes; i++)
                {
                    GameObject treePrefab = Resources.Load<GameObject>($"Trees/treePrefab{i + 1}");
                    if (treePrefab != null)
                    {
                        TreePrototypes.Add(treePrefab);
                    }
                    else
                    {
                        Debug.LogError($"Tree prefab '{i + 1}' not found in Resources/Trees folder.");
                    }
                }
            }
        }
    }
}

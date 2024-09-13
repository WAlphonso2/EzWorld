﻿using Assets.Scripts.MapGenerator.Maps;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        public override IEnumerator Generate(WorldInfo worldInfo)
        {
            LoadSettings(worldInfo.terrainData.treeGeneratorData);

            if (Randomize)
            {
                Offset = Random.Range(0f, 9999f);
            }

            List<TreePrototype> treePrototypes = new List<TreePrototype>();
            foreach (var t in TreePrototypes)
            {
                treePrototypes.Add(new TreePrototype() { prefab = t });
            }

            UnityEngine.TerrainData terrainData = Terrain.activeTerrain.terrainData;
            terrainData.treePrototypes = treePrototypes.ToArray();

            terrainData.treeInstances = new TreeInstance[0];

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

            for (int x = 0; x < terrainData.alphamapWidth; x++)
            {
                for (int y = 0; y < terrainData.alphamapHeight; y++)
                {
                    float height = terrainData.GetHeight(x, y);
                    float heightScaled = height / terrainData.size.y;
                    float xScaled = (x + Random.Range(-1f, 1f)) / terrainData.alphamapWidth;
                    float yScaled = (y + Random.Range(-1f, 1f)) / terrainData.alphamapHeight;
                    float steepness = terrainData.GetSteepness(xScaled, yScaled);

                    float noiseStep = Random.Range(0f, 1f);
                    float noiseVal = noiseMap[x, y];

                    if
                        (
                         noiseStep < Density &&
                         noiseVal < IslandSize &&
                         steepness < MaxSteepness &&
                         height > MinLevel &&
                         height < MaxLevel
                        )
                        {
                            treePos.Add(new Vector3(xScaled, heightScaled, yScaled));
                        }
                }
            }

            TreeInstance[] treeInstances = new TreeInstance[treePos.Count];

            for (int ii = 0; ii < treeInstances.Length; ii++)
            {
                treeInstances[ii].position = treePos[ii];
                treeInstances[ii].prototypeIndex = Random.Range(0, treePrototypes.Count);
                treeInstances[ii].color = new Color(Random.Range(100, 255), Random.Range(100, 255), Random.Range(100, 255));
                treeInstances[ii].lightmapColor = Color.white;
                treeInstances[ii].heightScale = 1.0f + Random.Range(-0.25f, 0.5f);
                treeInstances[ii].widthScale = 1.0f + Random.Range(-0.5f, 0.25f);
            }
            terrainData.treeInstances = treeInstances;

            Debug.Log(treeInstances.Length + " trees were created");

            yield return null;
        }

        public override void Clear()
        {
            try
            {
                //Terrain.activeTerrain.terrainData.treePrototypes = null;
                Terrain.activeTerrain.terrainData.treeInstances = new TreeInstance[0];
            }
            catch { }
        }

        private void LoadSettings(TreeGeneratorData data)
        {
            if (data == null)
            {
                Debug.Log("TreeGeneratorData is null");
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

            // Clear existing trees
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
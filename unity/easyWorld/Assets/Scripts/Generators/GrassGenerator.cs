using Assets.Scripts.MapGenerator.Maps;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.MapGenerator.Generators
{
    public class GrassGenerator : Generator
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

        public List<Texture2D> GrassTextures;

        public override IEnumerator Generate(WorldInfo worldInfo, int terrainIndex)
        {
            CustomTerrainData terrainData = worldInfo.terrainsData[terrainIndex];  // Get the terrain data for the current terrain
            LoadSettings(terrainData.grassGeneratorData);

            if (Randomize)
            {
                Offset = Random.Range(0f, 9999f);
            }

            // Use GetTerrainByIndexOrCreate to ensure the terrain exists
            Terrain terrain = TerrainGenerator.GetTerrainByIndexOrCreate(terrainIndex, terrainData.heightsGeneratorData.width, terrainData.heightsGeneratorData.depth, terrainData.heightsGeneratorData.height);

            if (terrain == null)
            {
                Debug.LogError($"No terrain found or created for index {terrainIndex}");
                yield break;
            }

            UnityEngine.TerrainData terrainUnityData = terrain.terrainData;

            List<DetailPrototype> detailPrototypes = new List<DetailPrototype>();
            foreach (var g in GrassTextures)
            {
                var detailPrototype = new DetailPrototype
                {
                    prototypeTexture = g,
                    renderMode = DetailRenderMode.GrassBillboard,
                    healthyColor = Color.green,
                    dryColor = Color.yellow,
                    minHeight = 0.2f,
                    maxHeight = 1.0f,
                    minWidth = 0.2f,
                    maxWidth = 1f
                };
                detailPrototypes.Add(detailPrototype);
            }

            terrainUnityData.detailPrototypes = detailPrototypes.ToArray();
            terrainUnityData.SetDetailResolution(terrainUnityData.alphamapWidth, 8);

            float[,] noiseMap = new PerlinMap()
            {
                Size = terrainUnityData.alphamapWidth,
                Octaves = Octaves,
                Scale = Scale,
                Offset = Offset,
                Persistance = Persistence,
                Lacunarity = Lacunarity
            }.Generate(out float maxLocalNoiseHeight, out float minLocalNoiseHeight);

            for (int i = 0; i < terrainUnityData.detailPrototypes.Length; i++)
            {
                int[,] detailLayer = new int[terrainUnityData.detailWidth, terrainUnityData.detailHeight];

                for (int x = 0; x < terrainUnityData.alphamapWidth; x++)
                {
                    for (int y = 0; y < terrainUnityData.alphamapHeight; y++)
                    {
                        float height = terrainUnityData.GetHeight(x, y);
                        float steepness = terrainUnityData.GetSteepness(x / (float)terrainUnityData.alphamapWidth, y / (float)terrainUnityData.alphamapHeight);
                        float noiseValue = noiseMap[x, y];

                        // Check grass placement conditions
                        if (noiseValue < IslandSize && steepness < MaxSteepness && height > MinLevel && height < MaxLevel)
                        {
                            if (Random.Range(0f, 1f) < Density)
                            {
                                // Set the detailLayer at this point to max grass density
                                detailLayer[x, y] = Mathf.RoundToInt(Density * 1000); // Max density
                            }
                        }
                        else
                        {
                            detailLayer[x, y] = 0; // No grass
                        }
                    }
                }

                terrainUnityData.SetDetailLayer(0, 0, i, detailLayer);
            }

            Debug.Log($"Grass generation completed for Terrain {terrainIndex}.");
            yield return null;
        }

        public override void Clear()
        {
            Terrain.activeTerrain.terrainData.detailPrototypes = null;
            Debug.Log("Grass cleared.");
        }

        private void LoadSettings(GrassGeneratorData data)
        {
            if (data == null)
            {
                Debug.LogError("GrassGeneratorData is null");
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

            GrassTextures.Clear();
            for (int i = 0; i < data.grassTextures; i++)
            {
                Texture2D grassTexture = Resources.Load<Texture2D>($"Grass/Grass {i + 1}");
                if (grassTexture != null)
                {
                    GrassTextures.Add(grassTexture);
                }
                else
                {
                    Debug.LogError($"Grass texture '{i + 1}' not found in Resources/Grass folder.");
                }
            }
        }
    }
}

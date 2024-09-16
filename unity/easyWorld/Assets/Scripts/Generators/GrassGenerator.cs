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

        public override IEnumerator Generate(WorldInfo worldInfo)
        {
            LoadSettings(worldInfo.terrainData.grassGeneratorData);

            if (Randomize)
            {
                Offset = Random.Range(0f, 9999f);
            }

            List<DetailPrototype> detailPrototypes = new List<DetailPrototype>();
            foreach (var g in GrassTextures)
            {
                var detailPrototype = new DetailPrototype
                {
                    prototypeTexture = g,
                    renderMode = DetailRenderMode.GrassBillboard,
                    healthyColor = Color.green,
                    dryColor = Color.yellow,
                    // Set the height for short and tall grass, with most grass being shorter
                    minHeight = 0.2f, 
                    maxHeight = 1.0f, 
                    minWidth = 0.2f,
                    maxWidth = 1f
                };
                detailPrototypes.Add(detailPrototype);
            }

            UnityEngine.TerrainData terrainData = Terrain.activeTerrain.terrainData;
            terrainData.detailPrototypes = detailPrototypes.ToArray();

            terrainData.SetDetailResolution(terrainData.alphamapWidth, 8);

            float[,] noiseMap = new PerlinMap()
            {
                Size = terrainData.alphamapWidth,
                Octaves = Octaves,
                Scale = Scale,
                Offset = Offset,
                Persistance = Persistence,
                Lacunarity = Lacunarity
            }.Generate(out float maxLocalNoiseHeight, out float minLocalNoiseHeight);

            for (int i = 0; i < terrainData.detailPrototypes.Length; i++)
            {
                int[,] detailLayer = new int[terrainData.detailWidth, terrainData.detailHeight];

                for (int x = 0; x < terrainData.alphamapWidth; x++)
                {
                    for (int y = 0; y < terrainData.alphamapHeight; y++)
                    {
                        float height = terrainData.GetHeight(x, y);
                        float steepness = terrainData.GetSteepness(x / (float)terrainData.alphamapWidth, y / (float)terrainData.alphamapHeight);
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

                terrainData.SetDetailLayer(0, 0, i, detailLayer);
            }

            Debug.Log("Grass generation completed.");
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

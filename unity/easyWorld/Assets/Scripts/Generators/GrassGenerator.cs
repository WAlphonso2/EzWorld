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
        [Range(1, 100)]
        public int Density = 10;
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
                detailPrototypes.Add(new DetailPrototype() { prototypeTexture = g });
            }

            UnityEngine.TerrainData terrainData = Terrain.activeTerrain.terrainData;
            terrainData.detailPrototypes = detailPrototypes.ToArray();

            float[,] noiseMap = new PerlinMap()
            {
                Size = terrainData.detailWidth,
                     Octaves = Octaves,
                     Scale = Scale,
                     Offset = Offset,
                     Persistance = Persistence,
                     Lacunarity = Lacunarity
            }.Generate();

            for (int i = 0; i < terrainData.detailPrototypes.Length; i++)
            {
                int[,] detailLayer = terrainData.GetDetailLayer(0, 0, terrainData.detailWidth, terrainData.detailHeight, i);

                for (int x = 0; x < terrainData.alphamapWidth; x++)
                {
                    for (int y = 0; y < terrainData.alphamapHeight; y++)
                    {
                        float height = terrainData.GetHeight(x, y);
                        float xScaled = (x + Random.Range(-1f, 1f)) / terrainData.alphamapWidth;
                        float yScaled = (y + Random.Range(-1f, 1f)) / terrainData.alphamapHeight;
                        float steepness = terrainData.GetSteepness(xScaled, yScaled);

                        if (noiseMap[x, y] < IslandSize && steepness < MaxSteepness && height > MinLevel && height < MaxLevel)
                        {
                            detailLayer[x, y] = Density;
                        }
                        else
                        {
                            detailLayer[x, y] = 0;
                        }
                    }
                }

                terrainData.SetDetailLayer(0, 0, i, detailLayer);
            }

            yield return null;
        }

        public override void Clear()
        {
            Terrain.activeTerrain.terrainData.detailPrototypes = null;
        }

        private void LoadSettings(GrassGeneratorData data)
        {
            if (data == null)
            {
                Debug.Log("GrassGeneratorData is null");
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

            // Clear existing grass textures
            GrassTextures.Clear();

            if (data.grassTextures > 0)
            {
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
}

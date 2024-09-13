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
        [Range(1, 1000)]  // Increased range to allow for higher density
        public int Density = 1000;  // Default density set to a higher value
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
                    dryColor = Color.yellow
                };
                detailPrototypes.Add(detailPrototype);
            }

            UnityEngine.TerrainData terrainData = Terrain.activeTerrain.terrainData;
            terrainData.detailPrototypes = detailPrototypes.ToArray();

            // Set a high detail resolution for denser grass placement
            int detailResolution = 1024;
            terrainData.SetDetailResolution(detailResolution, 8);

            // Generate Perlin noise map for grass placement
            float[,] noiseMap = new PerlinMap()
            {
                Size = terrainData.detailWidth,
                Octaves = Octaves,
                Scale = Scale,
                Offset = Offset,
                Persistance = Persistence,
                Lacunarity = Lacunarity
            }.Generate();

            // Grass counter to track the number of grass instances generated
            int grassCount = 0;

            // Adjust this multiplier to control how much noise affects the grass placement
            float noiseInfluenceMultiplier = Mathf.Lerp(1f, 0f, Density / 1000f);  // Less noise influence at max density

            for (int i = 0; i < terrainData.detailPrototypes.Length; i++)
            {
                int[,] detailLayer = terrainData.GetDetailLayer(0, 0, terrainData.detailWidth, terrainData.detailHeight, i);

                for (int x = 0; x < terrainData.detailWidth; x++)
                {
                    for (int y = 0; y < terrainData.detailHeight; y++)
                    {
                        float height = terrainData.GetHeight(x, y);
                        float xScaled = (x + Random.Range(-1f, 1f)) / terrainData.detailWidth;
                        float yScaled = (y + Random.Range(-1f, 1f)) / terrainData.detailHeight;
                        float steepness = terrainData.GetSteepness(xScaled, yScaled);
                        float noiseValue = noiseMap[x, y] * noiseInfluenceMultiplier;

                        // Adjust grass placement conditions for high density
                        if ((noiseValue < IslandSize || Density == 1000) && steepness < MaxSteepness && height > MinLevel && height < MaxLevel)
                        {
                            detailLayer[x, y] = Mathf.Clamp(Density, 1, 1000);
                            grassCount++;  // Count each grass instance placed
                        }
                        else
                        {
                            detailLayer[x, y] = 0;  // No grass if conditions aren't met
                        }
                    }
                }

                terrainData.SetDetailLayer(0, 0, i, detailLayer);
            }

            // Log the number of grass patches generated
            Debug.Log(grassCount + " grass patches were generated.");

            yield return null;
        }

        public override void Clear()
        {
            // Clear grass details
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

            // Load grass textures
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

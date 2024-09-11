using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.MapGenerator.Generators
{
    public class TexturesGenerator : MonoBehaviour, IGenerator
    {
        public List<_Texture> textures = new List<_Texture>();

        public void Generate()
        {
            if (textures == null || textures.Count == 0)
            {
                Debug.LogError("No textures assigned to TexturesGenerator.");
                return;
            }

            TerrainData terrainData = Terrain.activeTerrain.terrainData;

            // Create and assign Terrain Layers
            TerrainLayer[] terrainLayers = new TerrainLayer[textures.Count];

            for (int i = 0; i < textures.Count; i++)
            {
                TerrainLayer layer = new TerrainLayer();
                layer.diffuseTexture = textures[i].Texture;
                layer.tileSize = textures[i].Tilesize;
                terrainLayers[i] = layer;
            }

            // Apply terrain layers to the terrain
            terrainData.terrainLayers = terrainLayers;

            // Fill the splatmap array (alphamaps)
            float[,,] splatmaps = new float[terrainData.alphamapWidth, terrainData.alphamapHeight, terrainData.alphamapLayers];
            float terrainMaxHeight = terrainData.size.y;

            for (int y = 0; y < terrainData.alphamapHeight; y++)
            {
                for (int x = 0; x < terrainData.alphamapWidth; x++)
                {
                    float height = terrainData.GetHeight(x, y);
                    float heightScaled = height / terrainMaxHeight;
                    float steepness = terrainData.GetSteepness((float)x / terrainData.heightmapResolution, (float)y / terrainData.heightmapResolution);

                    // Loop through each layer and apply based on height and slope (this can be customized)
                    for (int i = 0; i < terrainData.alphamapLayers; i++)
                    {
                        if (textures[i].Type == 0) // Height-based texture (e.g., grass, desert)
                        {
                            splatmaps[y, x, i] = textures[i].HeightCurve.Evaluate(heightScaled);
                        }
                        else if (textures[i].Type == 1) // Angle-based texture (e.g., rocks, cliffs)
                        {
                            float angle = steepness / 90.0f;
                            splatmaps[y, x, i] = textures[i].AngleCurve.Evaluate(angle);
                        }

                        // Ensure the value is within the range [0, 1]
                        splatmaps[y, x, i] = Mathf.Clamp(splatmaps[y, x, i], 0, 1);
                    }
                }
            }

            // Apply the splatmaps to the terrain
            terrainData.SetAlphamaps(0, 0, splatmaps);
        }

        public void Clear()
        {
            // Clear textures
            textures = new List<_Texture>();
            Terrain.activeTerrain.terrainData.terrainLayers = null;
        }
    }

    [System.Serializable]
    public class _Texture
    {
        public Texture2D Texture { get; set; }
        public Vector2 Tilesize = new Vector2(1, 1);
        public int Type { get; set; } // 0 = Height-based, 1 = Angle-based
        public AnimationCurve HeightCurve = AnimationCurve.Linear(0.0f, 1.0f, 1.0f, 1.0f);
        public AnimationCurve AngleCurve = AnimationCurve.Linear(0.0f, 1.0f, 1.0f, 1.0f);
    }
}

// using System;
// using System.Collections.Generic;
// using UnityEngine;

// namespace Assets.Scripts.MapGenerator.Generators
// {
//     public class TexturesGenerator : MonoBehaviour, IGenerator
//     {
//         public List<_Texture> textures = new List<_Texture>();

//         public void Generate()
//         {
//             if (textures == null || textures.Count == 0)
//             {
//                 throw new NullReferenceException("Textures list is not set.");
//             }

//             TerrainData terrainData = Terrain.activeTerrain.terrainData;

//             // Create and assign Terrain Layers instead of splat prototypes
//             TerrainLayer[] terrainLayers = new TerrainLayer[textures.Count];

//             for (int i = 0; i < textures.Count; i++)
//             {
//                 TerrainLayer layer = new TerrainLayer();
//                 layer.diffuseTexture = textures[i].Texture;
//                 layer.tileSize = textures[i].Tilesize;

//                 // Set optional properties like normal maps, metallic, smoothness if needed
//                 terrainLayers[i] = layer;
//             }

//             // Assign the terrain layers to the terrain data
//             terrainData.terrainLayers = terrainLayers;

//             if (terrainData.alphamapResolution != terrainData.size.x)
//             {
//                 Debug.LogError("Alphamap resolution must fit the terrain size.");
//             }

//             // Fill the splatmap array (alphamaps)
//             float[,,] splatmaps = new float[terrainData.alphamapWidth, terrainData.alphamapHeight, terrainData.alphamapLayers];
//             float terrainMaxHeight = terrainData.size.y;

//             for (int y = 0; y < terrainData.alphamapHeight; y++)
//             {
//                 for (int x = 0; x < terrainData.alphamapWidth; x++)
//                 {
//                     float height = terrainData.GetHeight(x, y);
//                     float heightScaled = height / terrainMaxHeight;

//                     float xS = x / terrainData.heightmapResolution;
//                     float yS = y / terrainData.heightmapResolution;

//                     float steepness = terrainData.GetSteepness(xS, yS);
//                     float angleScaled = steepness / 90.0f;

//                     for (int i = 0; i < terrainData.alphamapLayers; i++)
//                     {
//                         switch (textures[i].Type)
//                         {
//                             case 0: // Height-based texture
//                                 splatmaps[y, x, i] = textures[i].HeightCurve.Evaluate(heightScaled);
//                                 break;
//                             case 1: // Angle-based texture (for steep slopes, e.g., cliffs)
//                                 splatmaps[y, x, i] = textures[i].AngleCurve.Evaluate(angleScaled);
//                                 break;
//                         }

//                         // Ensure the value is within [0, 1]
//                         splatmaps[y, x, i] = Mathf.Clamp(splatmaps[y, x, i], 0, 1);
//                     }
//                 }
//             }

//             // Apply the splatmap to the terrain
//             terrainData.SetAlphamaps(0, 0, splatmaps);
//         }

//         public void Clear()
//         {
//             // Clear textures
//             textures = new List<_Texture>();

//             // Remove terrain layers
//             Terrain.activeTerrain.terrainData.terrainLayers = null;
//         }
//     }

//     [System.Serializable]
//     public class _Texture
//     {
//         public Texture2D Texture { get; set; }
//         public Vector2 Tilesize = new Vector2(1, 1);
//         public int Type { get; set; } // 0 = Height-based, 1 = Angle-based
//         public AnimationCurve HeightCurve = AnimationCurve.Linear(0.0f, 1.0f, 1.0f, 1.0f);
//         public AnimationCurve AngleCurve = AnimationCurve.Linear(0.0f, 1.0f, 1.0f, 1.0f);
//     }
// }

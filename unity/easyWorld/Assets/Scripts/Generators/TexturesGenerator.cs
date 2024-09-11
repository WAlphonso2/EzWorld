using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TexturesGenerator : Generator
{
    public List<_Texture> textures = new List<_Texture>();

    public override IEnumerator Generate(WorldInfo worldInfo)
    {
        LoadSettings(worldInfo.terrainData.texturesGeneratorDataList);

        if (textures == null || textures.Count == 0)
        {
            Debug.LogError("No textures assigned to TexturesGenerator.");
            yield break;
        }

        UnityEngine.TerrainData terrainData = Terrain.activeTerrain.terrainData;

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

        yield return null;
    }

    public override void Clear()
    {
        // Clear textures
        textures = new List<_Texture>();
        Terrain.activeTerrain.terrainData.terrainLayers = null;
    }

    private void LoadSettings(List<TexturesGeneratorData> data)
    {
        if (data == null)
        {
            Debug.Log("TextureGeneratorDataList is null");
            return;
        }

        foreach (var textureData in data)
        {
            Texture2D texture = Resources.Load<Texture2D>($"Textures/{textureData.texture}");
            if (texture != null)
            {
                _Texture newTexture = new _Texture
                {
                    Texture = texture,
                    Tilesize = new Vector2(textureData.tileSizeX, textureData.tileSizeY),
                    Type = 0,
                    HeightCurve = TerrainGenerator.GetHeightCurveFromType(textureData.heightCurve)
                };
                textures.Add(newTexture);
            }
            else
            {
                Debug.LogError($"Texture '{textureData.texture}' not found in Resources/Textures folder.");
            }
        }
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
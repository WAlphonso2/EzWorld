using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TexturesGenerator : Generator
{
    public List<_Texture> textures = new List<_Texture>();

    public override IEnumerator Generate(WorldInfo worldInfo, int terrainIndex)
    {
        // Load specific textures settings for this terrain
        LoadSettings(worldInfo.terrainsData[terrainIndex].texturesGeneratorDataList);

        if (textures == null || textures.Count == 0)
        {
            Debug.LogError("No textures assigned to TexturesGenerator.");
            yield break;
        }

        // Use GetTerrainByIndexOrCreate to ensure the terrain exists
        Terrain terrain = TerrainGenerator.GetTerrainByIndexOrCreate(terrainIndex, 
            worldInfo.terrainsData[terrainIndex].heightsGeneratorData.width, 
            worldInfo.terrainsData[terrainIndex].heightsGeneratorData.depth, 
            worldInfo.terrainsData[terrainIndex].heightsGeneratorData.height);

        if (terrain == null)
        {
            Debug.LogError($"No terrain found or created for index {terrainIndex}");
            yield break;
        }

        UnityEngine.TerrainData terrainData = terrain.terrainData;

        // Apply Terrain Layers
        TerrainLayer[] terrainLayers = new TerrainLayer[textures.Count];
        for (int i = 0; i < textures.Count; i++)
        {
            TerrainLayer layer = new TerrainLayer();
            layer.diffuseTexture = textures[i].Texture;
            layer.tileSize = textures[i].Tilesize;
            terrainLayers[i] = layer;

            Debug.Log($"Assigning texture {i}: {textures[i].Texture.name} with tile size {textures[i].Tilesize}");
        }

        terrainData.terrainLayers = terrainLayers;

        // Ensure the number of alphamap layers matches terrain layers
        if (terrainData.alphamapLayers != textures.Count)
        {
            Debug.LogError($"Mismatch in terrain layers ({textures.Count}) and alphamap layers ({terrainData.alphamapLayers}) count.");
            yield break;
        }

        // Generate splatmap
        float[,,] splatmaps = new float[terrainData.alphamapWidth, terrainData.alphamapHeight, terrainData.alphamapLayers];
        float terrainMaxHeight = terrainData.size.y;

        Debug.Log($"Starting splatmap generation for terrain {terrainIndex} with resolution {terrainData.alphamapWidth}x{terrainData.alphamapHeight}.");

        for (int y = 0; y < terrainData.alphamapHeight; y++)
        {
            for (int x = 0; x < terrainData.alphamapWidth; x++)
            {
                // Get the height and steepness at the current point
                float height = terrainData.GetHeight(x, y);
                float heightScaled = height / terrainMaxHeight;
                float steepness = terrainData.GetSteepness((float)x / terrainData.heightmapResolution, (float)y / terrainData.heightmapResolution);

                for (int i = 0; i < terrainData.alphamapLayers; i++)
                {
                    if (textures[i].Type == 0)  // Height-based texture
                    {
                        splatmaps[y, x, i] = textures[i].HeightCurve.Evaluate(heightScaled);
                    }
                    else if (textures[i].Type == 1)  // Angle-based texture
                    {
                        float angle = steepness / 90.0f;
                        splatmaps[y, x, i] = textures[i].AngleCurve.Evaluate(angle);
                    }

                    // Clamp the values to ensure they are between 0 and 1
                    splatmaps[y, x, i] = Mathf.Clamp(splatmaps[y, x, i], 0, 1);
                }
            }
        }

        // Apply the splatmaps to the terrain
        terrainData.SetAlphamaps(0, 0, splatmaps);

        Debug.Log($"Finished generating textures for terrain {terrainIndex}");

        yield return null;
    }



    public override void Clear()
    {
        // Clear the internal textures list
        textures = new List<_Texture>();

        // Ensure there's an active terrain and terrain data before attempting to clear terrain layers
        if (Terrain.activeTerrain != null && Terrain.activeTerrain.terrainData != null)
        {
            Terrain.activeTerrain.terrainData.terrainLayers = null;
            Debug.Log("Terrain layers cleared.");
        }
        else
        {
            Debug.LogWarning("No active terrain or terrain data found to clear textures.");
        }
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
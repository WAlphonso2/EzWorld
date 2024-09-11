using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WorldInfo
{
    public TerrainData terrainData;
}

[System.Serializable]
public class TerrainData
{
    public HeightsGeneratorData heightsGeneratorData;
    public List<TexturesGeneratorData> texturesGeneratorDataList;
    public TreeGeneratorData treeGeneratorData;
    public GrassGeneratorData grassGeneratorData;
    public WaterGeneratorData waterGeneratorData;
}

[System.Serializable]
public class TexturesGeneratorData
{
    public string texture;
    public string heightCurve;
    public float tileSizeX;
    public float tileSizeY;
}

[System.Serializable]
public class HeightsGeneratorData
{
    public int width;
    public int height;
    public int depth;
    public int octaves;
    public float scale;
    public float lacunarity;
    public float persistence;
    public float heightCurveOffset;
    public string heightCurve;
    public float falloffDirection;
    public float falloffRange;
    public bool useFalloffMap;
    public bool randomize;
    public bool autoUpdate;
}



[System.Serializable]
public class TreeGeneratorData
{
    public int octaves;
    public float scale;
    public float lacunarity;
    public float persistence;
    public float offset;
    public float minLevel;
    public float maxLevel;
    public float maxSteepness;
    public float islandSize;
    public float density;
    public bool randomize;
    public int treePrototypes;
    public override string ToString()
    {
        return $"{octaves} {scale} {lacunarity} {persistence} {offset} {minLevel} {maxLevel} {maxSteepness} {islandSize} {density} {randomize} {treePrototypes}";
    }
}

[System.Serializable]
public class GrassGeneratorData
{
    public int octaves;
    public float scale;
    public float lacunarity;
    public float persistence;
    public float offset;
    public float minLevel;
    public float maxLevel;
    public float maxSteepness;
    public float islandSize;
    public int density;
    public bool randomize;
    public int grassTextures;
}

[System.Serializable]
public class WaterGeneratorData
{
    public string waterType;
    public float waterLevel;
    public Vector2 riverWidthRange;
    public bool randomize;
    public bool autoUpdate;
}
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WorldInfo
{
    public TerrainData terrainData;
    public float[,] heightMap;
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
    public string texture = "none";
    public string heightCurve = "smooth";
    public float tileSizeX = 10;
    public float tileSizeY = 10;
}

[System.Serializable]
public class HeightsGeneratorData
{
    public int width = 1024;
    public int height = 1024;
    public int depth = 100;
    public int octaves = 4;
    public float scale = 100;
    public float lacunarity = 2;
    public float persistence = .5f;
    public float heightCurveOffset = .3f;
    public string heightCurve = "linear";
    public float falloffDirection = 3;
    public float falloffRange = 3;
    public bool useFalloffMap = true;
    public bool randomize = false;
    public bool autoUpdate = true;
}



[System.Serializable]
public class TreeGeneratorData
{
    public int octaves = 3;
    public float scale = 1;
    public float lacunarity = 2;
    public float persistence = .5f;
    public float offset = .2f;
    public float minLevel = .1f;
    public float maxLevel = .9f;
    public float maxSteepness = 45;
    public float islandSize = 1;
    public float density = 5;
    public bool randomize = false;
    public int treePrototypes = 3;
}

[System.Serializable]
public class GrassGeneratorData
{
    public int octaves = 3;
    public float scale = .8f;
    public float lacunarity = 2;
    public float persistence = .5f;
    public float offset = .3f;
    public float minLevel = .1f;
    public float maxLevel = 1;
    public float maxSteepness = 45;
    public float islandSize = 1;
    public int density = 20;
    public bool randomize = false;
    public int grassTextures = 2;
}

[System.Serializable]
public class WaterGeneratorData
{
    public string waterType = "none";
    public float waterLevel = 20;
    public Vector2 riverWidthRange = new(1024, 1024);
    public bool randomize = true;
    public bool autoUpdate = true;
}
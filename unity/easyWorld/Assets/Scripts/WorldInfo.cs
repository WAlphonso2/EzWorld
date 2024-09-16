using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WorldInfo
{
    public CustomTerrainData terrainData;
    public float[,] heightMap;
}

[System.Serializable]
public class CustomTerrainData
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
    public float ShallowDepth = 1f;
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
    public int octaves = 4;
    public float scale = 40;
    public float lacunarity = 2f;
    public float persistence = 0.5f;
    public float offset = 100f;
    public float minLevel = 0;
    public float maxLevel = 100;
    public float maxSteepness = 70;
    public float islandSize = 0;
    [Range(0, 1)]
    public float density = 0.5f;             
    public bool randomize = false;
    public bool autoUpdate = true;
    public int grassTextures = 1;
}

[System.Serializable]
public class WaterGeneratorData
{
    public string waterType = "none";
    public float waterLevel = 20;
    public Vector2 riverWidthRange = new(500, 500);
    public bool randomize = true;
    public bool autoUpdate = true;
}
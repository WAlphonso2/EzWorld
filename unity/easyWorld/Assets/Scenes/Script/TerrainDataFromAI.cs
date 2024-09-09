using System;
using System.Collections.Generic;  // Add this directive for List<>
using UnityEngine;

[System.Serializable]
public class TerrainDataFromAI
{
    public HeightsGeneratorData heightsGenerator;
    public List<TexturesGeneratorData> texturesGenerator;  // Ensure this is a list
    public TreeGeneratorData treeGenerator;
    public GrassGeneratorData grassGenerator;
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

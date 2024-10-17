using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WorldInfo
{
    // List to store multiple terrains
    public List<CustomTerrainData> terrainsData;
    public float[,] heightMap; 
    public List<ObjectGeneratorData> objectList;
    public CityGeneratorData cityData;
    public AtmosphereGeneratorData atmosphereGeneratorData;
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
public class CityGeneratorData
{
    public string citySize = "Small";
    public bool withSatelliteCity = false;
    public bool borderFlat = false;
    public bool withDowntownArea = true;
    public float downtownSize = 100f;
    public bool addTrafficSystem = true;
    public string trafficHand = "RightHand";
}

[System.Serializable]
public class TexturesGeneratorData
{
    public string texture = "none";
    public string heightCurve = "constant";
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
    public string heightCurve = "easeout";
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
    public float density = 0.5f;
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
    public Vector2 riverWidthRange = new Vector2(700, 600);
    public bool randomize = true;
    public bool autoUpdate = true;
}

[System.Serializable]
public class ObjectGeneratorData
{
    public float x = 0;
    public float y = 0;
    public float Rx = 0;
    public float Ry = 0;
    public float Rz = 0;
    public string name = "";
    public float scale = 1.0f;
}

[System.Serializable]
public class AtmosphereGeneratorData
{
    public float timeOfDay = 12;
    public float sunSize = .05f;
    public Color skyTint = Color.gray;
    public float atmosphericThickness = 1;
    public float exposure = 1;
    public float fogIntensity = 0;
    public Color fogColor = Color.gray;
}
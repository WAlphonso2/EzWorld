using System.Collections.Generic;
using UnityEngine;

public class TerrainGeneration : MonoBehaviour
{
    private static IDictionary<TerrainMaterial, Material> materialDictionary;

    [Header("Standard")]
    public int length = 128;
    public int width = 128;
    public int height = 30;
    public TerrainMaterial terrainMaterial = TerrainMaterial.Sand;
    private TerrainMaterial activeTerrainMaterial;

    [Header("Macro Perlin Noise")]
    [Range(0, 1)]
    public float macroPerlinNoiseWeight = .7f;
    public float macroPerlinNoiseScale = 5;
    private float macroPerlinNoiseXOffset;
    private float macroPerlinNoiseYOffset;

    [Header("Mid Perlin Noise")]
    [Range(0, 1)]
    public float midPerlinNoiseWeight = .25f;
    public float midPerlinNoiseScale = 5;
    private float midPerlinNoiseXOffset;
    private float midPerlinNoiseYOffset;

    [Header("Micro Perlin Noise (Ground Rougness)")]
    [Range(0, 1)]
    public float microPerlinNoiseWeight = .05f;
    [Range(1, 30)]
    public float microPerlinNoiseScale = 5;
    private float microPerlinNoiseXOffset;
    private float microPerlinNoiseYOffset;

    [Header("Regeneration")]
    public bool continuallyRegenerate = false;
    public bool regenerate = false;

    [Header("Smooth Perlin Noise")]
    public bool smoothPerlinNoiseRegeneration = false;
    public float smootherPerlinNoiseSpeed = 35;

    private GameObject terrainObject;
    private Terrain terrain;
    private TerrainData terrainData;

    void Start()
    {
        LoadMaterials();
    }

    void LoadMaterials()
    {
        materialDictionary = new Dictionary<TerrainMaterial, Material>()
        {
            { TerrainMaterial.Sand, Resources.Load<Material>("Sand") },
            { TerrainMaterial.Dirt, Resources.Load<Material>("Dirt") },
            { TerrainMaterial.Snow, Resources.Load<Material>("Snow") },
            { TerrainMaterial.Grass, Resources.Load<Material>("Grass") }
        };
    }

    public void CreateNewTerrainObject()
    {
        CheckPerlinNoiseWeights();
        UpdatePerlinNoiseSeed();

        terrainData = new TerrainData();
        UpdateTerrainData();

        terrainObject = Terrain.CreateTerrainGameObject(terrainData);
        terrain = terrainObject.GetComponent<Terrain>();

        LoadSelectedMaterial();
    }

    void UpdateTerrainData()
    {
        terrainData.heightmapResolution = width + 1;
        terrainData.size = new Vector3(width, height, length);
        terrainData.SetHeights(0, 0, GenerateHeightMap());
    }

    void LoadSelectedMaterial()
    {
        try
        {
            terrain.materialTemplate = materialDictionary[terrainMaterial];
            activeTerrainMaterial = terrainMaterial;
        }
        catch
        {
            terrain.materialTemplate = new Material(Shader.Find("Unlit/Color"))
            {
                color = Color.black
            };
            activeTerrainMaterial = TerrainMaterial.None;
        }
    }

    void CheckPerlinNoiseWeights()
    {
        if (Mathf.Abs(macroPerlinNoiseWeight + midPerlinNoiseWeight + microPerlinNoiseWeight - 1) > .001f)
        {
            macroPerlinNoiseWeight = .7f;
            midPerlinNoiseWeight = .25f;
            microPerlinNoiseWeight = .05f;
        }
    }

    float[,] GenerateHeightMap()
    {
        int heightMapWidth = width + 1;
        int heightMapLength = length + 1;

        float[,] heightMap = new float[heightMapWidth, heightMapLength];

        for (int x = 0; x < heightMapWidth; x++)
        {
            for (int z = 0; z < heightMapLength; z++)
            {
                heightMap[x, z] = CalculatePositionHeight(x, z);
            }
        }

        return heightMap;
    }

    float CalculatePositionHeight(int x, int z)
    {
        // scale positions [0,1]
        float xPos = (float)x / width;
        float zPos = (float)z / length;

        float macroPerlinNoise = Mathf.PerlinNoise(xPos * macroPerlinNoiseScale + macroPerlinNoiseXOffset, zPos * macroPerlinNoiseScale + macroPerlinNoiseYOffset);
        float midPerlinNoise = Mathf.PerlinNoise(xPos * midPerlinNoiseScale + midPerlinNoiseXOffset, zPos * midPerlinNoiseScale + midPerlinNoiseYOffset);
        float microPerlinNoise = Mathf.PerlinNoise(xPos * microPerlinNoiseScale + microPerlinNoiseXOffset, zPos * microPerlinNoiseScale + microPerlinNoiseYOffset);

        float weightedSum = macroPerlinNoise * macroPerlinNoiseWeight + midPerlinNoise * midPerlinNoiseWeight + microPerlinNoise * microPerlinNoiseWeight;
        return Mathf.Clamp(weightedSum, 0, 1);
    }

    void UpdateTerrain()
    {
        CheckPerlinNoiseWeights();
        UpdatePerlinNoiseSeed();

        if (terrainMaterial != activeTerrainMaterial)
            LoadSelectedMaterial();

        UpdateTerrainData();
    }

    void UpdatePerlinNoiseSeed()
    {
        // acts as random seed for perlin noise as it moves around the perlin noise image

        if (smoothPerlinNoiseRegeneration)
        {
            float dp = smootherPerlinNoiseSpeed * Time.deltaTime / 100f;
            macroPerlinNoiseXOffset += dp;
            macroPerlinNoiseYOffset += dp;
            midPerlinNoiseXOffset += dp;
            midPerlinNoiseYOffset += dp;
            microPerlinNoiseXOffset += dp;
            microPerlinNoiseYOffset += dp;
        }
        else
        {
            macroPerlinNoiseXOffset = Random.Range(-100000, 100000);
            macroPerlinNoiseYOffset = Random.Range(-100000, 100000);
            midPerlinNoiseXOffset = Random.Range(-100000, 100000);
            midPerlinNoiseYOffset = Random.Range(-100000, 100000);
            microPerlinNoiseXOffset = Random.Range(-100000, 100000);
            microPerlinNoiseYOffset = Random.Range(-100000, 100000);
        }
    }

    void Update()
    {
        if (regenerate)
        {
            UpdateTerrain();
            regenerate = continuallyRegenerate;
        }
    }
}

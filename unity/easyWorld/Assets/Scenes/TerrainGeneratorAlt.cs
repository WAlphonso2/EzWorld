using System.Collections.Generic;
using UnityEngine;

public class TerrainGeneratorAlt : MonoBehaviour
{
    private static IDictionary<TerrainMaterial, Material> materialDictionary;

    [Header("Standard")]
    public int length = 128;
    public int width = 128;
    private int height = 30;
    public TerrainMaterial terrainMaterial = TerrainMaterial.Sand;
    private TerrainMaterial activeTerrainMaterial;

    [Header("Terrain Parameters")]
    [Range(0, 1)]
    public float mountainousness = 0;

    [Header("Ridge Perlin Noise")]
    public float ridgePerlinNoiseScale = 1f;
    private float ridgePerlinNoiseXOffset;
    private float ridgePerlinNoiseYOffset;

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

        CreateNewTerrainObject();
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

    void CreateNewTerrainObject()
    {
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

        // ridge perlin noise
        float ridgePerlinNoise = Mathf.Pow(Mathf.PerlinNoise(xPos * ridgePerlinNoiseScale + ridgePerlinNoiseXOffset, zPos * ridgePerlinNoiseScale + ridgePerlinNoiseYOffset), 2);

        return Mathf.Clamp(ridgePerlinNoise, 0, 1);
    }

    void UpdateTerrain()
    {
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
            ridgePerlinNoiseXOffset += dp;
            ridgePerlinNoiseYOffset += dp;
        }
        else
        {
            ridgePerlinNoiseXOffset = Random.Range(-100000, 100000);
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

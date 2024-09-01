using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    private static IDictionary<TerrainMaterial, Material> materialDictionary;

    public int length = 128;
    public int width = 128;
    public int height = 30;
    public float perlinNoiseScale = 5;

    public TerrainMaterial terrainMaterial = TerrainMaterial.Sand;

    private GameObject terrain;

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
        TerrainData terrainData = new TerrainData();
        UpdateTerrainData(terrainData);

        terrain = Terrain.CreateTerrainGameObject(terrainData);

        LoadSelectedMaterial();
    }

    void UpdateTerrainData(TerrainData terrainData)
    {
        terrainData.heightmapResolution = width + 1;
        terrainData.size = new Vector3(width, height, length);
        terrainData.SetHeights(0, 0, GenerateHeightMap());
    }

    void LoadSelectedMaterial()
    {
        Terrain t = terrain.GetComponent<Terrain>();
        try
        {
            t.materialTemplate = materialDictionary[terrainMaterial];
        }
        catch
        {
            t.materialTemplate = new Material(Shader.Find("Unlit/Color"))
            {
                color = Color.black
            };
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

        float perlinNoiseHeight = Mathf.PerlinNoise(xPos * perlinNoiseScale, zPos * perlinNoiseScale);
        return Mathf.Clamp(perlinNoiseHeight, 0, 1);
    }

    void Update()
    {

    }
}

public enum TerrainMaterial
{
    Grass,
    Dirt,
    Snow,
    Sand
}

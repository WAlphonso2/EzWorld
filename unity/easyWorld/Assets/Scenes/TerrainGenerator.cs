using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    public int length = 128;
    public int width = 128;
    public int height = 30;

    private GameObject terrain;

    void Start()
    {
        CreateNewTerrainObject();
    }

    void CreateNewTerrainObject()
    {
        TerrainData terrainData = new TerrainData();
        UpdateTerrainData(terrainData);

        terrain = Terrain.CreateTerrainGameObject(terrainData);
    }

    void UpdateTerrainData(TerrainData terrainData)
    {
        terrainData.heightmapResolution = width + 1;
        terrainData.size = new Vector3(width, height, length);
        terrainData.SetHeights(0, 0, GenerateHeightMap());
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

        float perlinNoiseHeight = Mathf.PerlinNoise(xPos, zPos);
        return Mathf.Clamp(perlinNoiseHeight, 0, 1);
    }

    void Update()
    {

    }
}

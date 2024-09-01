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
    }

    void Update()
    {

    }
}

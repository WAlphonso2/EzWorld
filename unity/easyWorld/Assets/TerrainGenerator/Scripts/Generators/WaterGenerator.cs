using UnityEngine;

namespace Assets.Scripts.MapGenerator.Generators
{
    public class WaterGenerator : MonoBehaviour, IGenerator
    {
        public GameObject waterPrefab;  
        public float waterLevel = 20f;  
        public Vector2 riverWidthRange = new Vector2(5f, 20f);  
        public bool randomize = true;  
        public bool autoUpdate = true;

        public void Generate()
        {
            GenerateWater("river");  // Default to river generation, this can be changed
        }

        public void GenerateWater(string waterType)
        {
            switch (waterType.ToLower())
            {
                case "river":
                    GenerateRiver();
                    break;
                case "lake":
                case "ocean":
                    GenerateLakeOrOcean();
                    break;
                default:
                    Debug.LogWarning("Unknown water type: " + waterType);
                    break;
            }
        }

        private void GenerateRiver()
        {
            Terrain terrain = Terrain.activeTerrain;
            TerrainData terrainData = terrain.terrainData;

            // Define river properties
            Vector3 riverStart = new Vector3(
                Random.Range(0, terrainData.size.x), 
                0, 
                Random.Range(0, terrainData.size.z));
            riverStart.y = terrain.SampleHeight(riverStart) + 1f;

            float riverWidth = Random.Range(riverWidthRange.x, riverWidthRange.y);

            // Carve the river into the terrain
            CarveRiverPath(terrainData, riverStart, riverWidth);

            // Place water in the river path
            GameObject river = Instantiate(waterPrefab, riverStart, Quaternion.identity);
            river.transform.localScale = new Vector3(riverWidth, 1, terrainData.size.z);
            river.transform.position = riverStart;

            Debug.Log("River generated at: " + riverStart);
        }

        private void GenerateLakeOrOcean()
        {
            Terrain terrain = Terrain.activeTerrain;
            TerrainData terrainData = terrain.terrainData;

            // Define lake/ocean properties
            Vector3 lakePosition = new Vector3(
                Random.Range(terrainData.size.x * 0.2f, terrainData.size.x * 0.8f), 
                waterLevel, 
                Random.Range(terrainData.size.z * 0.2f, terrainData.size.z * 0.8f));
            lakePosition.y = terrain.SampleHeight(lakePosition) + 1f;

            // Carve the lake/ocean into the terrain
            CarveLakeOrOcean(terrainData, lakePosition);

            // Place water in the lake/ocean depression
            GameObject lake = Instantiate(waterPrefab, lakePosition, Quaternion.identity);
            lake.transform.localScale = new Vector3(terrainData.size.x, waterLevel, terrainData.size.z);

            Debug.Log("Lake or ocean generated at: " + lakePosition);
        }

        private void CarveRiverPath(TerrainData terrainData, Vector3 startPoint, float width)
        {
            // Adjust the terrain heightmap for a shallow river path
            int startX = (int)(startPoint.x / terrainData.size.x * terrainData.heightmapResolution);
            int startZ = (int)(startPoint.z / terrainData.size.z * terrainData.heightmapResolution);
            int riverWidth = Mathf.FloorToInt(width / terrainData.size.x * terrainData.heightmapResolution);

            float[,] heights = terrainData.GetHeights(0, 0, terrainData.heightmapResolution, terrainData.heightmapResolution);

            // Create a shallow river by lowering the heightmap slightly
            for (int x = startX - riverWidth; x < startX + riverWidth; x++)
            {
                for (int z = startZ - riverWidth; z < startZ + riverWidth; z++)
                {
                    if (x >= 0 && x < terrainData.heightmapResolution && z >= 0 && z < terrainData.heightmapResolution)
                    {
                        float distanceToCenter = Vector2.Distance(new Vector2(x, z), new Vector2(startX, startZ));

                        if (distanceToCenter <= riverWidth)
                        {
                            // Make the path shallow
                            heights[x, z] = Mathf.Max(0.1f, heights[x, z] - 0.02f);  // Ensure shallow depth
                        }
                    }
                }
            }

            terrainData.SetHeights(0, 0, heights);
        }

        private void CarveLakeOrOcean(TerrainData terrainData, Vector3 center)
        {
            // Carve a lake or ocean depression
            int centerX = (int)(center.x / terrainData.size.x * terrainData.heightmapResolution);
            int centerZ = (int)(center.z / terrainData.size.z * terrainData.heightmapResolution);
            int lakeRadius = Random.Range(20, 50);  // Adjust radius based on lake size

            float[,] heights = terrainData.GetHeights(0, 0, terrainData.heightmapResolution, terrainData.heightmapResolution);

            for (int x = centerX - lakeRadius; x < centerX + lakeRadius; x++)
            {
                for (int z = centerZ - lakeRadius; z < centerZ + lakeRadius; z++)
                {
                    if (x >= 0 && x < terrainData.heightmapResolution && z >= 0 && z < terrainData.heightmapResolution)
                    {
                        float distanceToCenter = Vector2.Distance(new Vector2(x, z), new Vector2(centerX, centerZ));
                        if (distanceToCenter <= lakeRadius)
                        {
                            // Lower the terrain to form a depression
                            heights[x, z] = Mathf.Max(0.05f, heights[x, z] - 0.1f * (1 - (distanceToCenter / lakeRadius))); 
                        }
                    }
                }
            }

            terrainData.SetHeights(0, 0, heights);
        }

        public void Clear()
        {
            // Clear water objects if needed
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.MapGenerator.Generators
{
    public class WaterGenerator : Generator
    {
        public GameObject waterPrefab;
        public float waterLevel = 4f; // Default water level
        public bool autoUpdate = true;
        public RiverGenerator riverGenerator;
        
        public override IEnumerator Generate(WorldInfo worldInfo)
        {
            Terrain terrain = Terrain.activeTerrain;

            if (terrain == null)
            {
                Debug.LogError("No active terrain found.");
                yield break;
            }

            UnityEngine.TerrainData terrainData = terrain.terrainData;

            // Check water type from the user's input
            string waterType = worldInfo.terrainData.waterGeneratorData.waterType.ToLower();

            switch (waterType)
            {
                case "river":
                    if (riverGenerator != null)
                    {
                        // Generate river first using RiverGenerator
                        yield return StartCoroutine(riverGenerator.Generate(worldInfo));
                    }
                    FillRiverWithWaterSingleObject(terrain, terrainData, riverGenerator.mainRiverPathPoints);
                    GenerateOcean(terrain, terrainData);
                    break;
                case "lake":
                    GenerateLake(terrain, terrainData);
                    break;
                case "ocean":
                    GenerateOcean(terrain, terrainData);
                    break;
                default:
                    Debug.LogWarning("Unknown or unsupported water type: " + waterType);
                    break;
            }

            yield return null;
        }

public void FillRiverWithWaterSingleObject(Terrain terrain, UnityEngine.TerrainData terrainData, List<Vector2> riverPath)
{
    if (riverPath == null || riverPath.Count < 2)
    {
        Debug.LogError("River path is empty or too short to generate water.");
        return;
    }

    // Get the terrain size to scale the water mesh
    float terrainWidth = terrainData.size.x;
    float terrainHeight = terrainData.size.z;

    // Create a new GameObject for the river water
    GameObject riverWater = Instantiate(waterPrefab);
    riverWater.name = "RiverWater";

    // Create a procedural mesh for the river water
    MeshFilter meshFilter = riverWater.GetComponent<MeshFilter>();
    if (meshFilter == null)
    {
        meshFilter = riverWater.AddComponent<MeshFilter>();
    }

    MeshRenderer meshRenderer = riverWater.GetComponent<MeshRenderer>();
    if (meshRenderer == null)
    {
        meshRenderer = riverWater.AddComponent<MeshRenderer>();
    }

    Mesh riverMesh = new Mesh();
    meshFilter.mesh = riverMesh;

    // Generate vertices for the river mesh
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();

    float riverWidth = this.riverGenerator.riverWidth;  // Use the river width defined in RiverGenerator
    float waterWidthFactor = 4f;  // Increase this factor to make the water visibly wider than the riverbed
    float waterOffset = 0.05f;  // Offset to place water slightly above the riverbed

    // Loop through the river path points
    for (int i = 0; i < riverPath.Count - 1; i++)
    {
        Vector2 currentPoint = riverPath[i];
        Vector2 nextPoint = riverPath[i + 1];

        // Sample the terrain height at the current and next points
        float currentHeight = terrain.SampleHeight(new Vector3(currentPoint.x / terrainData.heightmapResolution * terrainWidth, 0, currentPoint.y / terrainData.heightmapResolution * terrainHeight));
        float nextHeight = terrain.SampleHeight(new Vector3(nextPoint.x / terrainData.heightmapResolution * terrainWidth, 0, nextPoint.y / terrainData.heightmapResolution * terrainHeight));

        // Set the Y position of the water slightly above the terrain height
        Vector3 currentPoint3D = new Vector3(
            currentPoint.x / terrainData.heightmapResolution * terrainWidth,
            currentHeight + waterOffset,  // Place water slightly above the riverbed
            currentPoint.y / terrainData.heightmapResolution * terrainHeight
        );

        Vector3 nextPoint3D = new Vector3(
            nextPoint.x / terrainData.heightmapResolution * terrainWidth,
            nextHeight + waterOffset,  // Place water slightly above the riverbed
            nextPoint.y / terrainData.heightmapResolution * terrainHeight
        );

        // Calculate perpendicular vectors to create the river's width
        Vector3 direction = (nextPoint3D - currentPoint3D).normalized;
        Vector3 perpendicular = Vector3.Cross(direction, Vector3.up).normalized * riverWidth * waterWidthFactor * 0.5f;

        // Add vertices for the current and next segment, adjusting Y position based on the terrain height
        Vector3 leftCurrent = currentPoint3D - perpendicular;  // Left bank (make the water wider)
        Vector3 rightCurrent = currentPoint3D + perpendicular; // Right bank (make the water wider)
        Vector3 leftNext = nextPoint3D - perpendicular;        // Next left bank
        Vector3 rightNext = nextPoint3D + perpendicular;       // Next right bank

        // Add vertices for the current and next segment
        vertices.Add(leftCurrent);
        vertices.Add(rightCurrent);
        vertices.Add(leftNext);
        vertices.Add(rightNext);

        // Add triangles for the river mesh (two triangles per segment)
        int startIndex = i * 4;
        triangles.AddRange(new int[] {
            startIndex, startIndex + 1, startIndex + 2,  // First triangle
            startIndex + 1, startIndex + 3, startIndex + 2  // Second triangle
        });
    }

    // Assign the generated vertices and triangles to the mesh
    riverMesh.vertices = vertices.ToArray();
    riverMesh.triangles = triangles.ToArray();
    riverMesh.RecalculateNormals();

    // Adjust the water mesh to better fit the river, no manual positioning needed
    riverWater.transform.position = Vector3.zero;
    riverWater.transform.localScale = Vector3.one;

    Debug.Log("River filled with a wider water object.");
}

        private void GenerateLake(Terrain terrain, UnityEngine.TerrainData terrainData)
        {
            // Define lake properties
            int lakeRadius = 65;  // Define a reasonable radius for the lake
            Vector3 center = new Vector3(
                terrainData.size.x * 0.5f,
                waterLevel,
                terrainData.size.z * 0.5f
            );

            // Place water in a circular shape
            CarveLakeOrOcean(terrainData, center, lakeRadius);

            // Instantiate water over the lake
            GameObject lake = Instantiate(waterPrefab, center, Quaternion.identity);
            lake.transform.localScale = new Vector3(lakeRadius * 2, 1, lakeRadius * 2);

            // Apply sand texture to the shoreline
            ApplySandTexture(terrain, terrainData, center, lakeRadius);

            Debug.Log("Lake generated at: " + center);
        }

        public void GenerateOcean(Terrain terrain, UnityEngine.TerrainData terrainData)
        {
            // Oceans cover the entire terrain
            Vector3 oceanPosition = new Vector3(terrainData.size.x / 2, waterLevel, terrainData.size.z / 2);

            // Place water over the entire terrain
            GameObject ocean = Instantiate(waterPrefab, oceanPosition, Quaternion.identity);
            ocean.transform.localScale = new Vector3(terrainData.size.x, 1, terrainData.size.z);

            Debug.Log("Ocean generated.");
        }


        private void CarveLakeOrOcean(UnityEngine.TerrainData terrainData, Vector3 center, int radius)
        {
            int centerX = (int)(center.x / terrainData.size.x * terrainData.heightmapResolution);
            int centerZ = (int)(center.z / terrainData.size.z * terrainData.heightmapResolution);

            float[,] heights = terrainData.GetHeights(0, 0, terrainData.heightmapResolution, terrainData.heightmapResolution);

            // Define the width of the shoreline (a transition zone between land and water)
            int shorelineWidth = 50; // Increase the shoreline width for a smoother transition

            // Define how shallow or deep the lake should be
            float lakeDepthFactor = 0.05f; // Shallower lake

            // Frequency and amplitude for Perlin noise to adjust the lake's edge
            float noiseFrequency = 0.05f; // Adjust this for more or less variation
            float noiseAmplitude = 20f;   // Controls how far the lake edge varies

            // Carve a circular depression for the lake with a smooth shoreline
            for (int x = centerX - radius - shorelineWidth; x < centerX + radius + shorelineWidth; x++)
            {
                for (int z = centerZ - radius - shorelineWidth; z < centerZ + radius + shorelineWidth; z++)
                {
                    if (x >= 0 && x < terrainData.heightmapResolution && z >= 0 && z < terrainData.heightmapResolution)
                    {
                        float distanceToCenter = Vector2.Distance(new Vector2(x, z), new Vector2(centerX, centerZ));

                        // Add Perlin noise to the radius to create a more organic lake shape
                        float noise = Mathf.PerlinNoise(x * noiseFrequency, z * noiseFrequency) * noiseAmplitude;
                        float adjustedRadius = radius + noise;

                        if (distanceToCenter <= adjustedRadius) // Inside the lake radius, set to shallow lake bed height
                        {
                            // Use lakeDepthFactor to make the lake shallower
                            heights[z, x] = Mathf.Lerp(0.05f, (waterLevel - (lakeDepthFactor * terrainData.size.y)) / terrainData.size.y, SmoothStep(0, adjustedRadius, distanceToCenter));
                        }
                        else if (distanceToCenter <= adjustedRadius + shorelineWidth) // Shoreline area, create a smoother gradient
                        {
                            // SmoothStep transition for a softer shoreline
                            float t = SmoothStep(adjustedRadius, adjustedRadius + shorelineWidth, distanceToCenter);
                            heights[z, x] = Mathf.Lerp((waterLevel - (lakeDepthFactor * terrainData.size.y)) / terrainData.size.y, terrainData.GetHeight(x, z) / terrainData.size.y, t);
                        }
                    }
                }
            }

            terrainData.SetHeights(0, 0, heights);
        }

    private void ApplySandTexture(Terrain terrain, UnityEngine.TerrainData terrainData, Vector3 center, int radius)
    {
        int terrainWidth = terrainData.alphamapWidth;
        int terrainHeight = terrainData.alphamapHeight;

        // Get the existing alphamaps
        float[,,] alphaMap = terrainData.GetAlphamaps(0, 0, terrainWidth, terrainHeight);

        // Define the width of the shoreline for texture application
        int shorelineWidth = 35;

        int centerX = (int)(center.x / terrainData.size.x * terrainWidth);
        int centerZ = (int)(center.z / terrainData.size.z * terrainHeight);

        // Loop over the area near the lake and apply sand texture
        for (int x = centerX - radius - shorelineWidth; x < centerX + radius + shorelineWidth; x++)
        {
            for (int z = centerZ - radius - shorelineWidth; z < centerZ + radius + shorelineWidth; z++)
            {
                // Ensure x and z are within bounds
                if (x >= 0 && x < terrainWidth && z >= 0 && z < terrainHeight)
                {
                    float distanceToCenter = Vector2.Distance(new Vector2(x, z), new Vector2(centerX, centerZ));

                    // Check if we are near the shoreline
                    if (distanceToCenter <= radius + shorelineWidth && distanceToCenter >= radius)
                    {
                        // Apply sand texture based on the distance
                        float t = Mathf.InverseLerp(radius, radius + shorelineWidth, distanceToCenter);

                        // Sand texture index, assuming sand is at index 1 in the textures array
                        int sandTextureIndex = 1;

                        // Set the texture blending: gradually blend the sand texture
                        alphaMap[z, x, sandTextureIndex] = Mathf.Lerp(0, 1, t);

                        // Set other textures to 0 or blend them accordingly
                        for (int i = 0; i < terrainData.alphamapLayers; i++)
                        {
                            if (i != sandTextureIndex)
                            {
                                alphaMap[z, x, i] = 1 - alphaMap[z, x, sandTextureIndex];
                            }
                        }
                    }
                }
            }
        }

        // Apply the updated alphamap to the terrain
        terrainData.SetAlphamaps(0, 0, alphaMap);
    }



        private float SmoothStep(float edge0, float edge1, float x)
        {
            // Scale, bias and saturate x to 0-1 range
            x = Mathf.Clamp((x - edge0) / (edge1 - edge0), 0.0f, 1.0f);
            // Evaluate polynomial (smooth easing function)
            return x * x * (3 - 2 * x);
        }


        public override void Clear()
        {
            // Destroy any previously generated water objects
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
        }
    }
}

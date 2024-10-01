using Assets.Scripts.MapGenerator.Generators;
using System.Collections;
using System.Linq;
using UnityEngine;

public class CityGen : MonoBehaviour
{
    public GameObject cityGeneratorPrefab;  // Assign your CityGenerator prefab here
    public GameObject trafficSystemPrefab;  // Assign your TrafficSystem prefab here

    public IEnumerator GenerateCity(WorldInfo worldInfo, int terrainIndex, Terrain terrain)
    {
        // Calculate the center of the terrain to position the city
        Vector3 terrainCenter = new Vector3(
            terrain.transform.position.x + terrain.terrainData.size.x / 2,
            terrain.transform.position.y,
            terrain.transform.position.z + terrain.terrainData.size.z / 2
        );

        // Instantiate the city generator at the terrain's center
        if (cityGeneratorPrefab != null)
        {
            GameObject cityGenInstance = Instantiate(cityGeneratorPrefab, terrainCenter, Quaternion.identity);
            FCG.CityGenerator cityGen = cityGenInstance.GetComponent<FCG.CityGenerator>();

            if (cityGen != null)
            {
                // Get city size and generate the city
                int citySizeInt = GetCitySize(worldInfo.cityData.citySize);
                cityGen.GenerateCity(citySizeInt, worldInfo.cityData.withSatelliteCity, worldInfo.cityData.borderFlat);

                // Get the instantiated border object to calculate its bounding box
                GameObject borderObject = GetCityBorderObject(cityGen);
                if (borderObject != null)
                {
                    // Flatten the terrain based on the border size
                    yield return StartCoroutine(FlattenTerrainForCity(terrain, borderObject));
                }
                else
                {
                    Debug.LogError("City border object not found.");
                }

                // Move the City-Maker to the center of the terrain after generation
                MoveCityMakerToPosition(cityGen.cityMaker, terrainCenter);

                // If downtown area is enabled, generate buildings
                if (worldInfo.cityData.withDowntownArea)
                {
                    cityGen.GenerateAllBuildings(worldInfo.cityData.withDowntownArea, worldInfo.cityData.downtownSize);
                }

                // Add the traffic system after generating the city
                if (worldInfo.cityData.addTrafficSystem)
                {
                    yield return StartCoroutine(AddTrafficSystem(worldInfo.cityData.trafficHand, terrainCenter));
                }
            }
        }
        else
        {
            Debug.LogError("CityGeneratorPrefab is not assigned.");
        }

        yield return null;
    }

    // Move the City-Maker to the center of the terrain
    private void MoveCityMakerToPosition(GameObject cityMaker, Vector3 cityPosition)
    {
        if (cityMaker != null)
        {
            cityMaker.transform.position = cityPosition;
            Debug.Log("Moved City-Maker to position " + cityPosition);
        }
        else
        {
            Debug.LogError("City-Maker not found!");
        }
    }

    // Method to add traffic system after the city is generated
    private IEnumerator AddTrafficSystem(string trafficHand, Vector3 cityPosition)
    {
        // Instantiate or find the traffic system
        FCG.TrafficSystem trafficSystem = FindObjectOfType<FCG.TrafficSystem>();
        if (trafficSystem == null && trafficSystemPrefab != null)
        {
            GameObject trafficSystemObj = Instantiate(trafficSystemPrefab, cityPosition, Quaternion.identity);
            trafficSystem = trafficSystemObj.GetComponent<FCG.TrafficSystem>();
        }

        if (trafficSystem == null)
        {
            Debug.LogError("TrafficSystemPrefab not found or failed to instantiate.");
            yield break;
        }

        // Set traffic hand
        int hand = trafficHand.ToLower() == "righthand" ? 0 : 1;
        trafficSystem.DeffineDirection(hand);

        // Load cars based on traffic hand
        trafficSystem.LoadCars(hand);

        Debug.Log("Traffic system and vehicles added.");
        yield return null;
    }

    // Helper method to map city size string to integer
    private int GetCitySize(string size)
    {
        switch (size.ToLower())
        {
            case "small": return 2;
            case "medium": return 3;
            case "large": return 4;
            case "very large": return 5;
            default:
                Debug.LogError($"Unknown city size: {size}, defaulting to Small.");
                return 1; // Default to Small
        }
    }

    // Find the border object that was instantiated for the city
    private GameObject GetCityBorderObject(FCG.CityGenerator cityGen)
    {
        // Check if cityMaker exists
        if (cityGen.cityMaker != null)
        {
            foreach (Transform child in cityGen.cityMaker.transform)
            {
                // Assuming the border objects have specific names like "smallBorder" or "largeBorder"
                if (child.name.Contains("Border"))
                {
                    return child.gameObject;
                }
            }
        }
        return null;
    }

    // Flatten the terrain based on the bounding box of the city border
    private IEnumerator FlattenTerrainForCity(Terrain terrain, GameObject borderObject)
    {
        // Get the bounding box of the border object
        Bounds borderBounds = GetBoundingBox(borderObject);

        // Convert world position to terrain heightmap position
        TerrainData terrainData = terrain.terrainData;
        int terrainPosX = Mathf.RoundToInt((borderBounds.center.x - terrain.transform.position.x) / terrainData.size.x * terrainData.heightmapResolution);
        int terrainPosZ = Mathf.RoundToInt((borderBounds.center.z - terrain.transform.position.z) / terrainData.size.z * terrainData.heightmapResolution);

        int flattenWidth = Mathf.RoundToInt(borderBounds.size.x / terrainData.size.x * terrainData.heightmapResolution);
        int flattenHeight = Mathf.RoundToInt(borderBounds.size.z / terrainData.size.z * terrainData.heightmapResolution);

        // Get heightmap data
        int heightmapX = Mathf.Clamp(terrainPosX - flattenWidth / 2, 0, terrainData.heightmapResolution - flattenWidth);
        int heightmapZ = Mathf.Clamp(terrainPosZ - flattenHeight / 2, 0, terrainData.heightmapResolution - flattenHeight);
        float[,] heights = terrainData.GetHeights(heightmapX, heightmapZ, flattenWidth, flattenHeight);

        // Flatten terrain to the minimum height in the current region
        float minHeight = heights.Cast<float>().Min();
        for (int x = 0; x < heights.GetLength(0); x++)
        {
            for (int z = 0; z < heights.GetLength(1); z++)
            {
                heights[x, z] = minHeight;
            }
        }

        // Set the flattened heights back to the terrain
        terrainData.SetHeights(heightmapX, heightmapZ, heights);

        Debug.Log($"Flattened terrain for city based on border bounds: {borderBounds}");

        yield return null;
    }

    // Get the bounding box for the border object
    private Bounds GetBoundingBox(GameObject borderObject)
    {
        Renderer[] renderers = borderObject.GetComponentsInChildren<Renderer>();
        if (renderers.Length > 0)
        {
            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }
            return bounds;
        }
        else
        {
            Debug.LogError("No renderers found on border object.");
            return new Bounds(Vector3.zero, Vector3.zero);
        }
    }
}

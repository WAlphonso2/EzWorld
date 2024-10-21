using Assets.Scripts.MapGenerator.Generators;
using System.Collections;
using UnityEngine;

public class CityGen : MonoBehaviour
{
    public GameObject cityGeneratorPrefab;  // Assign your CityGenerator prefab here
    public GameObject trafficSystemPrefab;  // Assign your TrafficSystem prefab here
    public GameObject oceanPrefab;          // Assign your Ocean prefab here
    public Transform citySpawnPoint;
    public IEnumerator GenerateCity(WorldInfo worldInfo, int terrainIndex, Terrain terrain)
    {
        // Check if terrain is valid before proceeding
        if (terrain == null)
        {
            Debug.LogError("Invalid terrain. Cannot generate city.");
            yield break;
        }

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

                // Move the City-Maker to the center of the terrain after generation
                MoveCityMakerToPosition(cityGen.cityMaker, terrainCenter);

                // Generate buildings if required
                if (worldInfo.cityData.withDowntownArea)
                {
                    cityGen.GenerateAllBuildings(worldInfo.cityData.withDowntownArea, worldInfo.cityData.downtownSize);
                }

                // Add traffic system if enabled
                if (worldInfo.cityData.addTrafficSystem)
                {
                    yield return StartCoroutine(AddTrafficSystem(worldInfo.cityData.trafficHand, terrainCenter));
                }

                // Use the city's spawn point for player positioning
                citySpawnPoint = cityGenInstance.transform.Find("CitySpawnPoint");
                if (citySpawnPoint == null)
                {
                    Debug.LogWarning("CitySpawnPoint not found. Using city center for spawn.");
                    citySpawnPoint = new GameObject("CitySpawnPoint").transform;
                    citySpawnPoint.position = terrainCenter;
                }

                // Ensure all tasks are complete before removing terrain
                yield return new WaitForEndOfFrame();
                RemoveTerrain(terrain);  // Safely remove terrain now
                AddOcean(terrainCenter, cityGen.cityMaker.transform.position.y);
            }
        }
        else
        {
            Debug.LogError("CityGeneratorPrefab is not assigned.");
        }

        yield return null;
    }

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

    private IEnumerator AddTrafficSystem(string trafficHand, Vector3 cityPosition)
    {
        FCG.TrafficSystem trafficSystem = FindObjectOfType<FCG.TrafficSystem>();
        if (trafficSystem == null && trafficSystemPrefab != null)
        {
            GameObject trafficSystemObj = Instantiate(trafficSystemPrefab, cityPosition, Quaternion.identity);
            trafficSystem = trafficSystemObj.GetComponent<FCG.TrafficSystem>();
        }

        if (trafficSystem == null)
        {
            Debug.LogError("TrafficSystemPrefab not found.");
            yield break;
        }

        int hand = trafficHand.ToLower() == "righthand" ? 0 : 1;
        trafficSystem.DeffineDirection(hand);
        trafficSystem.LoadCars(hand);

        Debug.Log("Traffic system and vehicles added.");
        yield return null;
    }

    private void RemoveTerrain(Terrain terrain)
    {
        if (terrain != null)
        {
            Destroy(terrain.gameObject);
            Debug.Log("Terrain removed.");
        }
        else
        {
            Debug.LogError("Terrain not found.");
        }
    }

    private void AddOcean(Vector3 position, float cityHeight)
    {
        if (oceanPrefab != null)
        {
            Vector3 oceanPosition = new Vector3(position.x, cityHeight - 0.5f, position.z);
            Instantiate(oceanPrefab, oceanPosition, Quaternion.identity);
            Debug.Log("Ocean added at " + oceanPosition);
        }
        else
        {
            Debug.LogError("OceanPrefab is not assigned.");
        }
    }

    private int GetCitySize(string size)
    {
        switch (size.ToLower())
        {
            case "small": return 2;
            case "medium": return 3;
            case "large": return 4;
            case "very large": return 5;
            default:
                Debug.LogError($"Unknown city size: {size}. Defaulting to Small.");
                return 1;
        }
    }
}

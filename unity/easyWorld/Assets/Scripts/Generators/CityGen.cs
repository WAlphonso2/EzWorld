using Assets.Scripts.MapGenerator.Generators;
using System.Collections;
using UnityEngine;

public class CityGen : MonoBehaviour
{
    public GameObject cityGeneratorPrefab;  // Assign your CityGenerator prefab here
    public GameObject trafficSystemPrefab;  // Assign your TrafficSystem prefab here
    public GameObject oceanPrefab;          // Assign your Ocean prefab here

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

                // Now, remove the terrain and add the ocean (terrain is removed *after* everything else)
                yield return new WaitForEndOfFrame(); // Ensures generation has fully completed
                RemoveTerrain(terrain);
                AddOcean(terrainCenter, cityGen.cityMaker.transform.position.y);
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

    // Remove the terrain from the scene
    private void RemoveTerrain(Terrain terrain)
    {
        if (terrain != null)
        {
            Destroy(terrain.gameObject);
            Debug.Log("Terrain removed.");
        }
        else
        {
            Debug.LogError("Terrain not found!");
        }
    }

    // Add an ocean to replace the terrain, and place it 0.5 units below the city's height
    private void AddOcean(Vector3 position, float cityHeight)
    {
        if (oceanPrefab != null)
        {
            // Set the ocean 0.5 units below the city's height
            Vector3 oceanPosition = new Vector3(position.x, cityHeight - 0.5f, position.z);
            Instantiate(oceanPrefab, oceanPosition, Quaternion.identity);
            Debug.Log("Ocean added at position " + oceanPosition);
        }
        else
        {
            Debug.LogError("OceanPrefab is not assigned.");
        }
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
}

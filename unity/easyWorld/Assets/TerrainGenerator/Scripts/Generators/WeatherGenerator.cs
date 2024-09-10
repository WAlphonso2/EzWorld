using System.Collections;
using UnityEngine;

/// <summary>
/// Generates random clouds based on weather conditions such as sunny, rainy, windy, etc.
/// </summary>
public class CloudGenerator : MonoBehaviour
{
    public GameObject cloudPrefab;

    // Area to spawn clouds
    public float width = 100.0f;
    public float depth = 100.0f;

    public float stepMin = 1.0f;
    public float stepMax = 5.0f;

    [Range(0, 1)]
    public float threshold = 0.5f;  // The threshold for Perlin noise
    public float noiseStrength = 10.0f;

    private int cloudDensity = 10; // Default cloud density, adjustable based on weather

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Vector3 min = new Vector3(transform.position.x - width / 2, transform.position.y, transform.position.z - depth / 2);
        Vector3 max = new Vector3(transform.position.x + width / 2, transform.position.y, transform.position.z + depth / 2);
        Gizmos.DrawLine(min, new Vector3(max.x, min.y, min.z));
        Gizmos.DrawLine(new Vector3(max.x, min.y, min.z), max);
        Gizmos.DrawLine(max, new Vector3(min.x, max.y, max.z));
        Gizmos.DrawLine(new Vector3(min.x, max.y, max.z), min);
    }

    // Generate clouds with varying density based on the weather description
    public void SetCloudDensityBasedOnWeather(string weatherDescription)
    {
        switch (weatherDescription.ToLower())
        {
            case "sunny":
                cloudDensity = 2;  // Minimal clouds
                stepMin = 10.0f;
                stepMax = 15.0f;
                threshold = 0.7f;
                break;

            case "rainy":
                cloudDensity = 20;  // Dense clouds
                stepMin = 1.0f;
                stepMax = 3.0f;
                threshold = 0.3f;
                break;

            case "windy":
                cloudDensity = 10;  // Mid clouds
                stepMin = 3.0f;
                stepMax = 5.0f;
                threshold = 0.5f;
                break;

            case "stormy":
                cloudDensity = 25;  // Very dense clouds
                stepMin = 1.0f;
                stepMax = 2.0f;
                threshold = 0.2f;
                break;

            default:
                cloudDensity = 10;  // Default cloud amount for unknown weather
                stepMin = 5.0f;
                stepMax = 10.0f;
                threshold = 0.5f;
                break;
        }

        StartCoroutine(GenerateClouds());
    }

    IEnumerator GenerateClouds()
    {
        // Clear previous clouds if any
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        // Generate clouds based on density and threshold
        for (float x = 0; x < width; x += Random.Range(stepMin, stepMax))
        {
            for (float z = 0; z < depth; z += Random.Range(stepMin, stepMax))
            {
                float pcheck = Mathf.PerlinNoise(x / width * noiseStrength, z / depth * noiseStrength);
                if (pcheck >= threshold)
                {
                    GameObject cloud = Instantiate(cloudPrefab, 
                        new Vector3(x + (transform.position.x - width / 2), transform.position.y, z + (transform.position.z - depth / 2)),
                        Quaternion.identity) as GameObject;
                    
                    cloud.transform.SetParent(this.transform);
                    yield return null;  // Yield execution for the next frame
                }
            }
        }
    }
}

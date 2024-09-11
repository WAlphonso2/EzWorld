using Assets.Scripts.MapGenerator.Generators;
using CurvedPathGenerator;
using System.Collections;
using UnityEngine;

public class TerrainGenerator : Generator
{
    public HeightsGenerator heightsGenerator;
    public TexturesGenerator texturesGenerator;
    public TreeGenerator treeGenerator;
    public GrassGenerator grassGenerator;
    public WaterGenerator waterGenerator;
    public PathGenerator pathGenerator;

    public Camera initialCamera;
    public Camera thirdPersonCamera;
    public GameObject player;

    public override void Clear()
    {
        heightsGenerator.Clear();
        texturesGenerator.Clear();
        treeGenerator.Clear();
        grassGenerator.Clear();
        waterGenerator.Clear();
    }

    public override IEnumerator Generate(WorldInfo worldInfo)
    {
        // does order matter here or can they all be run in parallel?
        yield return StartCoroutine(heightsGenerator.Generate(worldInfo));
        yield return StartCoroutine(texturesGenerator.Generate(worldInfo));
        yield return StartCoroutine(treeGenerator.Generate(worldInfo));
        yield return StartCoroutine(grassGenerator.Generate(worldInfo));
        yield return StartCoroutine(waterGenerator.Generate(worldInfo));
        yield return StartCoroutine(SwitchToGamePlayMode());

        yield return null;
    }

    public static AnimationCurve GetHeightCurveFromType(string curveType)
    {
        switch (curveType.ToLower())
        {
            case "linear":
                return AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);

            case "constant":
                return AnimationCurve.Constant(0.0f, 1.0f, 1.0f);  // Constant value over time

            case "easein":
                return AnimationCurve.EaseInOut(0.0f, 0.0f, 1.0f, 1.0f);  // Ease-in (gradual start)

            case "easeout":
                return new AnimationCurve(new Keyframe(0.0f, 1.0f), new Keyframe(1.0f, 0.0f));  // Ease-out (gradual end)

            case "sine":
                return new AnimationCurve(
                    new Keyframe(0.0f, 0.0f),
                    new Keyframe(0.5f, 1.0f),
                    new Keyframe(1.0f, 0.0f));  // Simulate sine wave

            case "bezier":
                // Create a Bezier-like curve
                return new AnimationCurve(
                    new Keyframe(0.0f, 0.0f, 1.0f, 1.0f),
                    new Keyframe(0.5f, 1.0f, 0.0f, 0.0f),
                    new Keyframe(1.0f, 0.0f, -1.0f, -1.0f));

            default:
                Debug.LogWarning($"Unknown curve type: {curveType}, defaulting to linear.");
                return AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);  // Default to linear if unknown
        }
    }

    private IEnumerator SwitchToGamePlayMode()
    {
        Debug.Log("Waiting for terrain generation to finish...");

        Debug.Log("Terrain generation complete. Switching cameras...");

        // Switch to third-person camera after terrain is generated
        initialCamera.enabled = false;
        thirdPersonCamera.enabled = true;
        thirdPersonCamera.gameObject.SetActive(true);

        Debug.Log("Camera switched. Positioning player...");
        PositionPlayerOnTerrain();

        yield return null;
    }

    private void PositionPlayerOnTerrain()
    {
        Terrain terrain = Terrain.activeTerrain;

        UnityEngine.TerrainData terrainData = terrain.terrainData;

        // Get the center point of the terrain
        float centerX = terrainData.size.x / 2;
        float centerZ = terrainData.size.z / 2;

        // Randomly position the player within a certain range around the center
        float range = Mathf.Min(terrainData.size.x, terrainData.size.z) * 0.25f;
        float randomX = Random.Range(centerX - range, centerX + range);
        float randomZ = Random.Range(centerZ - range, centerZ + range);

        // Get the height at the random position on the terrain
        float yPos = terrain.SampleHeight(new Vector3(randomX, 0, randomZ)) + 1f;

        // Position the player
        Vector3 playerPosition = new Vector3(randomX, yPos, randomZ);
        player.transform.position = playerPosition;
        Debug.Log("Player positioned at " + player.transform.position);
    }
}

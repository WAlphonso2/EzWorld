using Assets.Scripts.MapGenerator.Generators;
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
    public RiverGenerator riverGenerator;
    public ObjectGenerator objectGenerator;

    public override void Clear()
    {
        for (int i = 0; i < 10; i++)
        {
            Terrain terrain = GameObject.Find($"Terrain_{i}")?.GetComponent<Terrain>();
            if (terrain != null)
            {
                heightsGenerator?.Clear();
                texturesGenerator?.Clear();
                treeGenerator?.Clear();
                grassGenerator?.Clear();
                waterGenerator?.Clear();
                pathGenerator?.Clear();
                riverGenerator?.Clear();
                objectGenerator?.Clear();

                Debug.Log($"Cleared terrain {i}");
            }
            else
            {
                Debug.LogWarning($"Terrain {i} not found, skipping clear.");
            }
        }
    }

    public override IEnumerator Generate(WorldInfo worldInfo, int terrainIndex)
    {
        Terrain terrain = GetTerrainByIndexOrCreate(terrainIndex, worldInfo.terrainsData[terrainIndex].heightsGeneratorData.width,
                                                    worldInfo.terrainsData[terrainIndex].heightsGeneratorData.depth,
                                                    worldInfo.terrainsData[terrainIndex].heightsGeneratorData.height);

        if (terrain == null)
        {
            Debug.LogError($"Failed to create or retrieve terrain at index {terrainIndex}");
            yield break;
        }

        yield return StartCoroutine(heightsGenerator.Generate(worldInfo, terrainIndex));
        yield return StartCoroutine(objectGenerator.Generate(worldInfo, terrainIndex));
        yield return StartCoroutine(texturesGenerator.Generate(worldInfo, terrainIndex));
        yield return StartCoroutine(treeGenerator.Generate(worldInfo, terrainIndex));
        yield return StartCoroutine(grassGenerator.Generate(worldInfo, terrainIndex));
        yield return StartCoroutine(pathGenerator.Generate(worldInfo, terrainIndex));
        yield return StartCoroutine(waterGenerator.Generate(worldInfo, terrainIndex));
    }

    public static Terrain GetTerrainByIndexOrCreate(int terrainIndex, int width, int depth, int height)
    {
        GameObject terrainGO = GameObject.Find($"Terrain_{terrainIndex}");
        Terrain terrain;

        if (terrainGO == null)
        {
            terrainGO = new GameObject($"Terrain_{terrainIndex}");
            terrain = terrainGO.AddComponent<Terrain>();
            TerrainCollider terrainCollider = terrainGO.AddComponent<TerrainCollider>();

            UnityEngine.TerrainData terrainData = new UnityEngine.TerrainData
            {
                heightmapResolution = width + 1,
                size = new Vector3(width, depth, height)
            };

            terrain.terrainData = terrainData;
            terrainCollider.terrainData = terrainData;

            terrainGO.transform.position = new Vector3(terrainIndex * width, 0, 0);
            Debug.Log($"Created new terrain at index {terrainIndex}");
        }
        else
        {
            terrain = terrainGO.GetComponent<Terrain>();
            Debug.Log($"Found existing terrain at index {terrainIndex}");
        }

        return terrain;
    }

    public static AnimationCurve GetHeightCurveFromType(string curveType)
    {
        switch (curveType.ToLower())
        {
            case "linear":
                return AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);
            case "constant":
                return AnimationCurve.Constant(0.0f, 1.0f, 1.0f);
            case "easein":
                return AnimationCurve.EaseInOut(0.0f, 0.0f, 1.0f, 1.0f);
            case "easeout":
                return new AnimationCurve(new Keyframe(0.0f, 1.0f), new Keyframe(1.0f, 0.0f));
            case "sine":
                return new AnimationCurve(new Keyframe(0.0f, 0.0f), new Keyframe(0.5f, 1.0f), new Keyframe(1.0f, 0.0f));
            case "bezier":
                return new AnimationCurve(new Keyframe(0.0f, 0.0f, 1.0f, 1.0f), new Keyframe(0.5f, 1.0f, 0.0f, 0.0f), new Keyframe(1.0f, 0.0f, -1.0f, -1.0f));
            default:
                Debug.LogWarning($"Unknown curve type: {curveType}, defaulting to linear.");
                return AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);
        }
    }
}

using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(HeightsGenerator))]
public class HeightsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector
        DrawDefaultInspector();

        HeightsGenerator heightsGenerator = (HeightsGenerator)target;

        GUILayout.Space(10); 

        if (GUILayout.Button("Generate"))
        {
            // Ensure there's a terrain in the scene
            if (Terrain.activeTerrain != null)
            {
                // Call the Generate method
                StartGeneration(heightsGenerator);
            }
            else
            {
                Debug.LogWarning("No active terrain found in the scene. Add a terrain first.");
            }
        }

        if (GUILayout.Button("Clear"))
        {
            // Call the Clear method
            heightsGenerator.Clear();
        }

        GUILayout.Space(10); 
    }

	private void StartGeneration(HeightsGenerator heightsGenerator)
	{
		// Ensure there's a terrain in the scene
		Terrain terrain = Terrain.activeTerrain;
		if (terrain == null)
		{
			Debug.LogWarning("No active terrain found in the scene. Add a terrain first.");
			return;
		}

		// Initialize WorldInfo and TerrainData
		WorldInfo worldInfo = new WorldInfo
		{
			terrainData = new TerrainData
			{
				heightsGeneratorData = new HeightsGeneratorData
				{
					width = heightsGenerator.Width,
					height = heightsGenerator.Height,
					depth = heightsGenerator.Depth,
					octaves = heightsGenerator.Octaves,
					scale = heightsGenerator.Scale,
					lacunarity = heightsGenerator.Lacunarity,
					persistence = heightsGenerator.Persistance,
					heightCurveOffset = heightsGenerator.Offset,
					falloffDirection = heightsGenerator.FalloffDirection,
					falloffRange = heightsGenerator.FalloffRange,
					useFalloffMap = heightsGenerator.UseFalloffMap,
					randomize = heightsGenerator.Randomize,
					autoUpdate = heightsGenerator.AutoUpdate,
					heightCurve = "linear" 
				}
			}
		};

		EditorCoroutineUtility.StartCoroutineOwnerless(heightsGenerator.Generate(worldInfo));
	}

}

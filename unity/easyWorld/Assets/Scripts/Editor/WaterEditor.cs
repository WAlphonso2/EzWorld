using UnityEngine;
using UnityEditor;
using Assets.Scripts.MapGenerator.Generators;
using Unity.EditorCoroutines.Editor;

[CustomEditor(typeof(WaterGenerator))]
public class WaterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Get reference to the WaterGenerator component
        WaterGenerator gen = (WaterGenerator)target;

        DrawDefaultInspector();

        GUILayout.Space(10);  

        // Water Type selection (options: river, lake, ocean)
        string[] waterTypes = new string[] { "river", "lake", "ocean" };

        // Find index of the current water type, fallback to 0 (river) if not found
        int selectedWaterTypeIndex = ArrayUtility.IndexOf(waterTypes, gen.waterPrefab != null ? gen.waterPrefab.name.ToLower() : "river");
        if (selectedWaterTypeIndex == -1) selectedWaterTypeIndex = 0; 

        selectedWaterTypeIndex = EditorGUILayout.Popup("Water Type", selectedWaterTypeIndex, waterTypes);

        // Apply the selected water type
        string selectedWaterType = waterTypes[selectedWaterTypeIndex];

        GUILayout.Space(10);  

        // Button to generate water
        if (GUILayout.Button("Generate Water"))
        {
            // Create a WorldInfo object for water generation
            WorldInfo worldInfo = new WorldInfo
            {
                terrainData = new TerrainData
                {
                    waterGeneratorData = new WaterGeneratorData
                    {
                        waterType = selectedWaterType,
                        waterLevel = gen.waterLevel,
                        riverWidthRange = gen.riverWidthRange,
                        randomize = gen.randomize,
                        autoUpdate = gen.autoUpdate
                    }
                }
            };

            // Start water generation
            EditorCoroutineUtility.StartCoroutineOwnerless(gen.Generate(worldInfo));
        }

        // Button to clear water
        if (GUILayout.Button("Clear Water"))
        {
            // Call the clear function without any other action
            gen.Clear();
        }

        GUILayout.Space(10); 

        // Auto-update functionality
        if (gen.autoUpdate && GUI.changed)
        {
            WorldInfo worldInfo = new WorldInfo
            {
                terrainData = new TerrainData
                {
                    waterGeneratorData = new WaterGeneratorData
                    {
                        waterType = selectedWaterType,
                        waterLevel = gen.waterLevel,
                        riverWidthRange = gen.riverWidthRange,
                        randomize = gen.randomize,
                        autoUpdate = gen.autoUpdate
                    }
                }
            };

            // Automatically generate water when parameters change, if auto-update is enabled
            EditorCoroutineUtility.StartCoroutineOwnerless(gen.Generate(worldInfo));
        }

        // Save changes made in the editor
        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }
}

using UnityEngine;
using UnityEditor;
using Unity.EditorCoroutines.Editor;
using Assets.Scripts.MapGenerator.Generators;
using System;
using System.Collections.Generic;

[CustomEditor(typeof(WaterGenerator))]
public class WaterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        WaterGenerator gen = (WaterGenerator)target;

        // Draw the waterPrefab field
        gen.waterPrefab = (GameObject)EditorGUILayout.ObjectField("Water Prefab", gen.waterPrefab, typeof(GameObject), true);

        // Draw waterLevel field
        gen.waterLevel = EditorGUILayout.FloatField("Water Level", gen.waterLevel);

        // Draw RiverGenerator field (for generating rivers with water)
        gen.riverGenerator = (RiverGenerator)EditorGUILayout.ObjectField("River Generator", gen.riverGenerator, typeof(RiverGenerator), true);

        GUILayout.Space(10);

        // Draw the rockPrefabs field for selecting multiple rock prefabs
        // SerializedProperty rockPrefabsProperty = serializedObject.FindProperty("rockPrefabs");
        // EditorGUILayout.PropertyField(rockPrefabsProperty, new GUIContent("Rock Prefabs"), true);

        // Draw the rockSpacing field for defining the spacing between rocks
        // gen.rockSpacing = EditorGUILayout.IntField("Rock Spacing", gen.rockSpacing);

        GUILayout.Space(10);

        // Choose water type (River, Lake, Ocean) from a dropdown menu
        string[] waterTypes = new string[] { "river", "lake", "ocean" };

        // Default to "river" if the prefab name doesn't match the available water types
        string currentWaterType = "river"; // Default to river if the water type is not specified

        // Get the selected water type from the dropdown
        int selectedWaterType = EditorGUILayout.Popup("Water Type", Array.IndexOf(waterTypes, currentWaterType), waterTypes);

        GUILayout.Space(10);

        // Validate if the river generator is set before allowing water generation
        if (gen.riverGenerator == null)
        {
            EditorGUILayout.HelpBox("Please assign a River Generator to generate the river.", MessageType.Warning);
        }

        // Generate water button
        if (gen.waterPrefab != null && gen.riverGenerator != null)
        {
            if (GUILayout.Button("Generate Water"))
            {
                // Check if the river has already been generated
                if (gen.riverGenerator.mainRiverPathPoints == null || gen.riverGenerator.mainRiverPathPoints.Count == 0)
                {
                    EditorGUILayout.HelpBox("The river path has not been generated. Please generate the river first.", MessageType.Warning);
                }
                else
                {
                    // Create WorldInfo for water generation
                    WorldInfo worldInfo = new WorldInfo
                    {
                        terrainsData = new List<CustomTerrainData>()
                    };

                    // Add terrain data for water generation
                    worldInfo.terrainsData.Add(new CustomTerrainData
                    {
                        waterGeneratorData = new WaterGeneratorData
                        {
                            waterLevel = gen.waterLevel,
                            waterType = waterTypes[selectedWaterType] // Assume waterTypes is a predefined list
                        }
                    });

                    // Start the water generation coroutine to fill the river path
                    EditorCoroutineUtility.StartCoroutineOwnerless(gen.Generate(worldInfo, 0)); // terrainIndex = 0
                }
            }
        }

        else
        {
            // Show warning if no water prefab or river generator is selected
            if (gen.waterPrefab == null)
            {
                EditorGUILayout.HelpBox("Please assign a Water Prefab.", MessageType.Warning);
            }
        }

        // Clear water button
        if (GUILayout.Button("Clear Water"))
        {
            gen.Clear();
        }

        GUILayout.Space(10);

        // Mark the WaterGenerator as dirty if any changes are made
        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }

        // Apply property modifications for serializedObject
        serializedObject.ApplyModifiedProperties();
    }
}

// using UnityEngine;
// using UnityEditor;
// using Unity.EditorCoroutines.Editor;
// using Assets.Scripts.MapGenerator.Generators;
// using System;

// [CustomEditor(typeof(WaterGenerator))]
// public class WaterEditor : Editor
// {
//     public override void OnInspectorGUI()
//     {
//         WaterGenerator gen = (WaterGenerator)target;

//         // Draw the waterPrefab field
//         gen.waterPrefab = (GameObject)EditorGUILayout.ObjectField("Water Prefab", gen.waterPrefab, typeof(GameObject), true);

//         // Draw waterLevel field
//         gen.waterLevel = EditorGUILayout.FloatField("Water Level", gen.waterLevel);

//         // Draw RiverGenerator field (for generating rivers with water)
//         gen.riverGenerator = (RiverGenerator)EditorGUILayout.ObjectField("River Generator", gen.riverGenerator, typeof(RiverGenerator), true);

//         GUILayout.Space(10);

//         // Choose water type (River, Lake, Ocean) from a dropdown menu
//         string[] waterTypes = new string[] { "river", "lake", "ocean" };

//         // Default to "river" if the prefab name doesn't match the available water types
//         string currentWaterType = "river"; // Default to river if the water type is not specified

//         // Get the selected water type from the dropdown
//         int selectedWaterType = EditorGUILayout.Popup("Water Type", Array.IndexOf(waterTypes, currentWaterType), waterTypes);

//         GUILayout.Space(10);

//         // Validate if the river generator is set before allowing water generation
//         if (gen.riverGenerator == null)
//         {
//             EditorGUILayout.HelpBox("Please assign a River Generator to generate the river.", MessageType.Warning);
//         }

//         // Generate water button
//         if (gen.waterPrefab != null && gen.riverGenerator != null)
//         {
//             if (GUILayout.Button("Generate Water"))
//             {
//                 // Check if the river has already been generated
//                 if (gen.riverGenerator.mainRiverPathPoints == null || gen.riverGenerator.mainRiverPathPoints.Count == 0)
//                 {
//                     EditorGUILayout.HelpBox("The river path has not been generated. Please generate the river first.", MessageType.Warning);
//                 }
//                 else
//                 {
//                     // Create WorldInfo for water generation
//                     WorldInfo worldInfo = new WorldInfo
//                     {
//                         terrainData = new CustomTerrainData  
//                         {
//                             waterGeneratorData = new WaterGeneratorData
//                             {
//                                 waterLevel = gen.waterLevel,
//                                 waterType = waterTypes[selectedWaterType]
//                             }
//                         }
//                     };

//                     // Start the water generation coroutine to fill the river path
//                     EditorCoroutineUtility.StartCoroutineOwnerless(gen.Generate(worldInfo));
//                 }
//             }
//         }
//         else
//         {
//             // Show warning if no water prefab or river generator is selected
//             if (gen.waterPrefab == null)
//             {
//                 EditorGUILayout.HelpBox("Please assign a Water Prefab.", MessageType.Warning);
//             }
//         }

//         // Clear water button
//         if (GUILayout.Button("Clear Water"))
//         {
//             gen.Clear();
//         }

//         GUILayout.Space(10);

//         // Mark the WaterGenerator as dirty if any changes are made
//         if (GUI.changed)
//         {
//             EditorUtility.SetDirty(target);
//         }
//     }
// }

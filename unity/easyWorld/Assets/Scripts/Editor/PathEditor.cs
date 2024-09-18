using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Unity.EditorCoroutines.Editor;

[CustomEditor(typeof(PathGenerator))]
public class PathEditor : Editor
{
    public override void OnInspectorGUI()
    {
        PathGenerator gen = (PathGenerator)target;

        // Draw Terrain field (drag and drop the terrain from the scene)
        gen.terrain = (Terrain)EditorGUILayout.ObjectField("Terrain", gen.terrain, typeof(Terrain), true);

        // Draw pathWidth field
        gen.pathWidth = EditorGUILayout.IntField("Path Width", gen.pathWidth);

        GUILayout.Space(10);

        gen.splitChance = EditorGUILayout.IntField("split Chance", gen.splitChance);

        GUILayout.Space(10);

        gen.splitSpread = EditorGUILayout.IntField("split Spread", (int)gen.splitSpread);

        GUILayout.Space(10);

        // Allow the user to select the path texture directly in the editor
        gen.selectedTexture = (Texture2D)EditorGUILayout.ObjectField("Select Path Texture", gen.selectedTexture, typeof(Texture2D), false);

        GUILayout.Space(10);

        // Draw curveSmoothness field
        gen.curveSmoothness = EditorGUILayout.IntField("Path Curve Smoothness", gen.curveSmoothness);

        GUILayout.Space(10);

        // Generate paths button (uses user inputs for path generation)
        if (gen.selectedTexture != null && gen.terrain != null)
        {
            if (GUILayout.Button("Generate Paths"))
            {
                // Prepare worldInfo with necessary data for path generation
                WorldInfo worldInfo = new WorldInfo
                {
                    terrainsData = new List<CustomTerrainData>(), // Initialize terrainsData list
                    heightMap = gen.terrain.terrainData.GetHeights(0, 0, 
                        gen.terrain.terrainData.heightmapResolution, 
                        gen.terrain.terrainData.heightmapResolution) // Store heightMap if necessary
                };

                // Add terrain data with heights and textures for path generation
                worldInfo.terrainsData.Add(new CustomTerrainData
                {
                    heightsGeneratorData = new HeightsGeneratorData
                    {
                        width = gen.terrain.terrainData.heightmapResolution,
                        height = gen.terrain.terrainData.heightmapResolution,
                        depth = 100 // Example value, modify as needed
                    },
                    texturesGeneratorDataList = new List<TexturesGeneratorData>()
                });

                // Add the selected texture to the texturesGeneratorDataList
                worldInfo.terrainsData[0].texturesGeneratorDataList.Add(new TexturesGeneratorData
                {
                    texture = gen.selectedTexture.name,
                    heightCurve = "linear", 
                    tileSizeX = 10, 
                    tileSizeY = 10 
                });

                // Start the path generation coroutine
                EditorCoroutineUtility.StartCoroutineOwnerless(gen.Generate(worldInfo, 0)); // Pass terrain index 0 for the first terrain
            }
        }

        else
        {
            // Show warnings if terrain or texture is not selected
            if (gen.terrain == null)
            {
                EditorGUILayout.HelpBox("Please assign a Terrain.", MessageType.Warning);
            }

            if (gen.selectedTexture == null)
            {
                EditorGUILayout.HelpBox("Please select a Texture for the path.", MessageType.Warning);
            }
        }

        // Clear paths button
        if (GUILayout.Button("Clear Paths"))
        {
            gen.Clear();
        }

        GUILayout.Space(10);

        // Mark the PathGenerator as dirty if any changes are made
        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }
}

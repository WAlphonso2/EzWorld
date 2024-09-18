using UnityEngine;
using UnityEditor;
using Unity.EditorCoroutines.Editor;
using Assets.Scripts.MapGenerator.Generators;
using System.Collections.Generic;

[CustomEditor(typeof(RiverGenerator))]
public class RiverEditor : Editor
{
    public override void OnInspectorGUI()
    {
        RiverGenerator gen = (RiverGenerator)target;

        // Draw Terrain field (drag and drop the terrain from the scene)
        gen.terrain = (Terrain)EditorGUILayout.ObjectField("Terrain", gen.terrain, typeof(Terrain), true);

        // Draw riverWidth field
        gen.riverWidth = EditorGUILayout.IntField("River Width", gen.riverWidth);

        // Draw riverDepth field
        gen.riverDepth = EditorGUILayout.FloatField("River Depth", gen.riverDepth);

        // Draw curveSmoothness field
        gen.curveSmoothness = EditorGUILayout.IntField("Curve Smoothness", gen.curveSmoothness);

        // Draw maxRiverSegments field
        gen.maxRiverSegments = EditorGUILayout.IntField("Max River Segments", gen.maxRiverSegments);

        // Draw min/max segment length fields
        gen.minSegmentLength = EditorGUILayout.FloatField("Min Segment Length", gen.minSegmentLength);
        gen.maxSegmentLength = EditorGUILayout.FloatField("Max Segment Length", gen.maxSegmentLength);

        // Draw slopeFactor field to control how steep the river is
        gen.slopeFactor = EditorGUILayout.FloatField("Slope Factor", gen.slopeFactor);

        GUILayout.Space(10);

        // Draw allowSplit field
        gen.allowSplit = EditorGUILayout.Toggle("Allow River Split", gen.allowSplit);
        if (gen.allowSplit)
        {
            gen.splitSpread = EditorGUILayout.FloatField("Split Spread", gen.splitSpread);
        }

        GUILayout.Space(10);

        // Draw fields for rock prefabs
        SerializedProperty rockArrayProperty = serializedObject.FindProperty("rockPrefabs");
        EditorGUILayout.PropertyField(rockArrayProperty, new GUIContent("Rock Prefabs"), true);  // Array of rock prefabs
        gen.rockSpacing = EditorGUILayout.IntField("Rock Spacing", gen.rockSpacing);

        GUILayout.Space(10);

        // Draw riverside texture field
        gen.riversideTexture = (Texture2D)EditorGUILayout.ObjectField("Riverside Texture", gen.riversideTexture, typeof(Texture2D), false);

        GUILayout.Space(10);

        // Draw a field for the water generator (so we can fill the river with water after generation)
        gen.waterGenerator = (WaterGenerator)EditorGUILayout.ObjectField("Water Generator", gen.waterGenerator, typeof(WaterGenerator), true);

        GUILayout.Space(10);

        // Generate river button
        if (gen.terrain != null && gen.waterGenerator != null)
        {
            if (GUILayout.Button("Generate River"))
            {
                // Create WorldInfo and assign the terrain data for height adjustment
                WorldInfo worldInfo = new WorldInfo
                {
                    terrainsData = new List<CustomTerrainData>()
                };

                // Add one terrain data entry for the current terrain
                worldInfo.terrainsData.Add(new CustomTerrainData
                {
                    heightsGeneratorData = new HeightsGeneratorData
                    {
                        width = gen.terrain.terrainData.heightmapResolution,
                        height = gen.terrain.terrainData.heightmapResolution,
                        depth = 100 // Example value
                    }
                });

                worldInfo.heightMap = gen.terrain.terrainData.GetHeights(0, 0,
                    gen.terrain.terrainData.heightmapResolution,
                    gen.terrain.terrainData.heightmapResolution);

                // Start the river generation coroutine
                EditorCoroutineUtility.StartCoroutineOwnerless(gen.Generate(worldInfo, 0)); // terrainIndex = 0
            }
        }

        else
        {
            // Show warning if no terrain or water generator is selected
            if (gen.terrain == null)
            {
                EditorGUILayout.HelpBox("Please assign a Terrain.", MessageType.Warning);
            }
            if (gen.waterGenerator == null)
            {
                EditorGUILayout.HelpBox("Please assign a Water Generator.", MessageType.Warning);
            }
        }

        // Clear river button
        if (GUILayout.Button("Clear River"))
        {
            gen.Clear();
        }

        GUILayout.Space(10);

        // Mark the RiverGenerator as dirty if any changes are made
        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }

        // Apply property modifications (needed for SerializedObject)
        serializedObject.ApplyModifiedProperties();
    }
}

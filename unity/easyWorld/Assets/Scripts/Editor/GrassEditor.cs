using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Unity.EditorCoroutines.Editor;
using Assets.Scripts.MapGenerator.Generators;

[CustomEditor(typeof(GrassGenerator))]
public class GrassEditor : Editor
{
    public override void OnInspectorGUI()
    {
        GrassGenerator gen = (GrassGenerator)target;

        DrawDefaultInspector();

        GUILayout.Space(10);

        EditorGUILayout.LabelField("Grass Textures", EditorStyles.boldLabel);

        // Ensure GrassTextures list is initialized
        if (gen.GrassTextures == null)
        {
            gen.GrassTextures = new List<Texture2D>();
        }

        // Display the grass textures with the ability to remove them
        for (int i = 0; i < gen.GrassTextures.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            gen.GrassTextures[i] = (Texture2D)EditorGUILayout.ObjectField($"Grass Texture {i + 1}", gen.GrassTextures[i], typeof(Texture2D), false);
            if (GUILayout.Button("Remove", GUILayout.Width(60)))
            {
                gen.GrassTextures.RemoveAt(i);
                i--; // Adjust index after removal
            }
            EditorGUILayout.EndHorizontal();
        }

        // Button to add new grass textures
        if (GUILayout.Button("Add Grass Texture"))
        {
            gen.GrassTextures.Add(null);
        }

        GUILayout.Space(10);

        // Generate Button: Starts grass generation
        if (GUILayout.Button("Generate"))
        {
            // Prepare worldInfo with the necessary grass generation data
            WorldInfo worldInfo = new WorldInfo
            {
                terrainsData = new List<CustomTerrainData>() // Initialize terrainsData list
            };

            // Add terrain data with grass generation information
            worldInfo.terrainsData.Add(new CustomTerrainData
            {
                grassGeneratorData = new GrassGeneratorData
                {
                    octaves = gen.Octaves,
                    scale = gen.Scale,
                    lacunarity = gen.Lacunarity,
                    persistence = gen.Persistence,
                    offset = gen.Offset,
                    minLevel = gen.MinLevel,
                    maxLevel = gen.MaxLevel,
                    maxSteepness = gen.MaxSteepness,
                    islandSize = gen.IslandSize,
                    density = gen.Density,
                    randomize = gen.Randomize,
                    grassTextures = gen.GrassTextures.Count // Use the number of grass textures
                }
            });

            // Start generating grass in a coroutine
            EditorCoroutineUtility.StartCoroutineOwnerless(gen.Generate(worldInfo, 0)); // Pass terrain index 0 for the first terrain
        }


        // Clear Button: Clears grass
        if (GUILayout.Button("Clear"))
        {
            gen.Clear();
        }

        // Ensure changes are saved
        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }
}

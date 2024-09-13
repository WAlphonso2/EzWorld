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

        // Draw default inspector properties (like path width, density, etc.)
        DrawDefaultInspector();

        GUILayout.Space(10);

        if (gen.PathTextures == null)
        {
            gen.PathTextures = new List<Texture2D>(); // Initialize the texture list if null
        }

        // Label for texture management
        EditorGUILayout.LabelField("Path Textures", EditorStyles.boldLabel);

        // Loop through each texture in the list
        for (int i = 0; i < gen.PathTextures.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();

            // Allow the user to select a texture from the project
            gen.PathTextures[i] = (Texture2D)EditorGUILayout.ObjectField($"Texture {i + 1}", gen.PathTextures[i], typeof(Texture2D), false);

            // Button to remove the texture
            if (GUILayout.Button("Remove", GUILayout.Width(60)))
            {
                gen.PathTextures.RemoveAt(i);
                i--;  // Decrement the index to handle the removal
            }

            EditorGUILayout.EndHorizontal();
        }

        // Button to add a new texture slot
        if (GUILayout.Button("Add Texture"))
        {
            gen.PathTextures.Add(null);  // Add a new empty texture slot
        }

        GUILayout.Space(10);

        // Button to start generating the paths
        if (gen.PathTextures.Count > 0)
        {
            if (GUILayout.Button("Generate Paths"))
            {
                // Prepare the WorldInfo object (assuming your PathGenerator uses this for terrain data)
                WorldInfo worldInfo = new WorldInfo
                {
                    terrainData = new TerrainData
                    {
                        // Assuming your height map and terrain data is populated elsewhere
                    },
                    heightMap = Terrain.activeTerrain.terrainData.GetHeights(0, 0, Terrain.activeTerrain.terrainData.heightmapResolution, Terrain.activeTerrain.terrainData.heightmapResolution)
                };

                // Start the path generation coroutine
                EditorCoroutineUtility.StartCoroutineOwnerless(gen.Generate(worldInfo));
            }
        }

        // Button to clear paths
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

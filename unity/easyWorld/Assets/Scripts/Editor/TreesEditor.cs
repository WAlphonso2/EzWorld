using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Assets.Scripts.MapGenerator.Generators;
using Unity.EditorCoroutines.Editor;

[CustomEditor(typeof(TreeGenerator))]
public class TreesEditor : Editor
{
    public override void OnInspectorGUI()
    {
        TreeGenerator gen = (TreeGenerator)target;

        DrawDefaultInspector();

        if (gen.TreePrototypes == null)
        {
            gen.TreePrototypes = new List<GameObject>();
        }

        // Label for Tree Prototypes management
        EditorGUILayout.LabelField("Tree Prototypes", EditorStyles.boldLabel);

        // Loop through each tree prefab in the list
        for (int i = 0; i < gen.TreePrototypes.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();

            // Allow users to select a tree prefab (GameObject) from the project
            gen.TreePrototypes[i] = (GameObject)EditorGUILayout.ObjectField($"Tree Prefab {i + 1}", gen.TreePrototypes[i], typeof(GameObject), false);

            // Button to remove the tree prefab
            if (GUILayout.Button("Remove", GUILayout.Width(60)))
            {
                gen.TreePrototypes.RemoveAt(i);
                i--;  
            }

            EditorGUILayout.EndHorizontal();
        }

        // Button to add a new tree prefab slot
        if (GUILayout.Button("Add Tree Prefab"))
        {
            gen.TreePrototypes.Add(null);  
        }

        GUILayout.Space(10);  

        // Generate button
        if (gen.TreePrototypes.Count > 0)
        {
            if (GUILayout.Button("Generate"))
            {
                // Create a WorldInfo object to pass to the generator
                WorldInfo worldInfo = new WorldInfo
                {
                    terrainData = new TerrainData
                    {
                        treeGeneratorData = new TreeGeneratorData
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
                            treePrototypes = gen.TreePrototypes.Count
                        }
                    }
                };

                // Start the tree generation coroutine using EditorCoroutineUtility
                EditorCoroutineUtility.StartCoroutineOwnerless(gen.Generate(worldInfo));
            }
        }

        // Clear button
        if (GUILayout.Button("Clear"))
        {
            gen.Clear();
        }

        GUILayout.Space(10);  

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }
}

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

        for (int i = 0; i < gen.GrassTextures.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            gen.GrassTextures[i] = (Texture2D)EditorGUILayout.ObjectField($"Grass Texture {i + 1}", gen.GrassTextures[i], typeof(Texture2D), false);
            if (GUILayout.Button("Remove", GUILayout.Width(60)))
            {
                gen.GrassTextures.RemoveAt(i);
                i--;
            }
            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("Add Grass Texture"))
        {
            gen.GrassTextures.Add(null);
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Generate"))
        {
            WorldInfo worldInfo = new WorldInfo
            {
                terrainData = new TerrainData
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
                        grassTextures = gen.GrassTextures.Count
                    }
                }
            };

            EditorCoroutineUtility.StartCoroutineOwnerless(gen.Generate(worldInfo));
        }

        if (GUILayout.Button("Clear"))
        {
            gen.Clear();
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }
}

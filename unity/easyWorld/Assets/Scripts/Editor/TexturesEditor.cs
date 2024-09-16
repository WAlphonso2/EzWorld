using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Unity.EditorCoroutines.Editor;

[CustomEditor(typeof(TexturesGenerator))]
public class TexturesEditor : Editor
{
    public override void OnInspectorGUI()
    {
        TexturesGenerator gen = (TexturesGenerator)target;

        // Loop through each texture in the list
        for (int i = 0; i < gen.textures.Count; i++)
        {
            var texture = gen.textures[i];

            EditorGUILayout.BeginHorizontal();

            // Display the texture selector and type dropdown
            texture.Texture = EditorGUILayout.ObjectField("Texture", texture.Texture, typeof(Texture2D), false) as Texture2D;
            texture.Type = EditorGUILayout.Popup(texture.Type, new string[] { "Height", "Angle" });

            EditorGUILayout.EndHorizontal();

            // Allow user to select custom curve based on the texture type
            if (texture.Type == 0) // Height-based texture
            {
                texture.HeightCurve = EditorGUILayout.CurveField("Height Curve", texture.HeightCurve);
            }
            else if (texture.Type == 1) // Angle-based texture
            {
                texture.AngleCurve = EditorGUILayout.CurveField("Angle Curve", texture.AngleCurve);
            }

            // Tile size adjustment field
            texture.Tilesize = EditorGUILayout.Vector2Field("Tilesize", texture.Tilesize);

            // Add some spacing
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }

        // Button to delete the last texture in the list
        if (gen.textures.Count > 0)
        {
            if (GUILayout.Button("Delete Last Texture"))
            {
                gen.textures.RemoveAt(gen.textures.Count - 1);
            }
        }

        // Button to add a new texture
        if (GUILayout.Button("Add Texture"))
        {
            gen.textures.Add(new _Texture());
        }

        // Button to generate textures
        if (gen.textures.Count > 0 && GUILayout.Button("Generate"))
        {
            EditorCoroutineUtility.StartCoroutineOwnerless(gen.Generate(CreateWorldInfoFromEditor(gen))); 
        }

        // Button to clear textures
        if (GUILayout.Button("Clear"))
        {
            gen.Clear();
        }

        // Update the inspector when changes are made
        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }

    // Create a WorldInfo object based on the textures set in the Editor
	private WorldInfo CreateWorldInfoFromEditor(TexturesGenerator gen)
	{
		WorldInfo worldInfo = new WorldInfo
		{
			terrainData = new CustomTerrainData
			{
				texturesGeneratorDataList = new List<TexturesGeneratorData>()
			}
		};

		// Populate the texturesGeneratorDataList with the data from the editor
		foreach (var tex in gen.textures)
		{
			string curveType = tex.Type == 0 ? "linear" : "easein";  
			if (tex.Type == 1)
			{
				curveType = "bezier"; 
			}

			var textureData = new TexturesGeneratorData
			{
				texture = tex.Texture ? tex.Texture.name : "none",
				heightCurve = curveType,  
				tileSizeX = tex.Tilesize.x,
				tileSizeY = tex.Tilesize.y
			};

			worldInfo.terrainData.texturesGeneratorDataList.Add(textureData);
		}

		return worldInfo;
	}

}

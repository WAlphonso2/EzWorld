using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PathGenerator : Generator
{
    public int pathWidth = 2;    // Width of the path
    public int pathDensity = 50; // Points per curve
    public List<Texture2D> PathTextures; // List of textures to paint the path

    private Terrain terrain;
    private float[,] heightMap;
    private int heightMapWidth;
    private int heightMapHeight;
    private float terrainDepth;

    public override void Clear()
    {
        foreach (Transform child in transform)
        {
            GameObject.Destroy(child.gameObject);
        }
    }

    public override IEnumerator Generate(WorldInfo worldInfo)
    {
        Debug.Log("Starting PathGenerator.Generate");
        yield return new WaitForSeconds(0.1f);

        terrain = Terrain.activeTerrain;
        if (terrain == null)
        {
            Debug.LogError("No active terrain found in the scene.");
            yield break;
        }
        else
        {
            Debug.Log("Active terrain found: " + terrain.name);
        }

        // Load height data from WorldInfo
        if (!LoadHeightData(worldInfo))
        {
            Debug.LogError("Height map data could not be loaded from WorldInfo.");
            yield break;
        }

        // Generate multiple random paths
        for (int i = 0; i < PathTextures.Count; i++)
        {
            Debug.Log("Generating random curved path with texture index: " + i);
            GenerateRandomCurvedPath(i);
        }

        // Apply textures to the paths after generation
        PaintPathOnTerrain();

        yield return null;
    }

    private bool LoadHeightData(WorldInfo worldInfo)
    {
        Debug.Log("Attempting to load height map data from WorldInfo...");

        if (worldInfo == null)
        {
            Debug.LogError("WorldInfo is null!");
            return false;
        }

        if (worldInfo.terrainData == null)
        {
            Debug.LogError("WorldInfo.terrainData is null! Ensure that the HeightsGenerator properly populates the terrainData.");
            return false;
        }

        if (worldInfo.heightMap == null)
        {
            Debug.LogError("Height map is null! Ensure HeightsGenerator generates and stores the heightMap correctly.");
            return false;
        }

        // If we get here, heightMap should not be null
        heightMap = worldInfo.heightMap;
        heightMapWidth = heightMap.GetLength(0);
        heightMapHeight = heightMap.GetLength(1);
        terrainDepth = worldInfo.terrainData.heightsGeneratorData.depth;

        Debug.Log($"Height map loaded successfully: width = {heightMapWidth}, height = {heightMapHeight}, depth = {terrainDepth}");
        return true;
    }


    private float GetHeightFromHeightMap(Vector3 position)
    {
        float normalizedX = position.x / terrain.terrainData.size.x;
        float normalizedZ = position.z / terrain.terrainData.size.z;

        int heightMapX = Mathf.Clamp(Mathf.FloorToInt(normalizedX * heightMapWidth), 0, heightMapWidth - 1);
        int heightMapZ = Mathf.Clamp(Mathf.FloorToInt(normalizedZ * heightMapHeight), 0, heightMapHeight - 1);

        float heightValue = heightMap[heightMapZ, heightMapX] * terrainDepth;
        Debug.Log($"Height value retrieved from height map: {heightValue} at position ({heightMapX}, {heightMapZ})");
        return heightValue;
    }

    private void GenerateRandomCurvedPath(int textureIndex)
    {
        Vector3 terrainSize = terrain.terrainData.size;
        Debug.Log($"Generating random curved path on terrain of size: {terrainSize}");

        Vector3 startPoint = new Vector3(
            Random.Range(0, terrainSize.x),
            0,
            Random.Range(0, terrainSize.z)
        );

        Vector3 endPoint = new Vector3(
            Random.Range(0, terrainSize.x),
            0,
            Random.Range(0, terrainSize.z)
        );

        Vector3 controlPoint1 = new Vector3(
            Random.Range(0, terrainSize.x),
            0,
            Random.Range(0, terrainSize.z)
        );

        Vector3 controlPoint2 = new Vector3(
            Random.Range(0, terrainSize.x),
            0,
            Random.Range(0, terrainSize.z)
        );

        Debug.Log($"Start point: {startPoint}, End point: {endPoint}, Control points: {controlPoint1}, {controlPoint2}");
        CreatePathWithBezier(startPoint, controlPoint1, controlPoint2, endPoint, textureIndex);
    }

    private void CreatePathWithBezier(Vector3 startPoint, Vector3 controlPoint1, Vector3 controlPoint2, Vector3 endPoint, int textureIndex)
    {
        Debug.Log($"Creating path with Bezier curve using texture index: {textureIndex}");

        for (float t = 0; t <= 1; t += 1.0f / pathDensity)
        {
            Vector3 interpolatedPoint = CalculateBezierPoint(t, startPoint, controlPoint1, controlPoint2, endPoint);

            float terrainHeight = GetHeightFromHeightMap(interpolatedPoint);
            Vector3 pathPosition = new Vector3(interpolatedPoint.x, terrainHeight, interpolatedPoint.z);

            Debug.Log($"Bezier path point at t={t}: {pathPosition}");
        }
    }

    private Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;

        Vector3 point = uuu * p0;
        point += 3 * uu * t * p1;
        point += 3 * u * tt * p2;
        point += ttt * p3;

        return point;
    }

    private void PaintPathOnTerrain()
    {
        UnityEngine.TerrainData terrainData = terrain.terrainData;

        float[,,] alphaMap = terrainData.GetAlphamaps(0, 0, terrainData.alphamapWidth, terrainData.alphamapHeight);
        int alphaMapWidth = terrainData.alphamapWidth;
        int alphaMapHeight = terrainData.alphamapHeight;

        Debug.Log("Painting path on terrain...");

        for (int i = 0; i < PathTextures.Count; i++)
        {
            int pathTextureIndex = GetUniqueTextureIndex(terrainData);

            Debug.Log($"Painting path with texture index: {pathTextureIndex}");

            for (float t = 0; t <= 1; t += 1.0f / pathDensity)
            {
                Vector3 interpolatedPoint = CalculateBezierPoint(t, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero);

                float normalizedX = (interpolatedPoint.x - terrain.transform.position.x) / terrainData.size.x;
                float normalizedZ = (interpolatedPoint.z - terrain.transform.position.z) / terrainData.size.z;

                int alphaX = Mathf.FloorToInt(normalizedX * alphaMapWidth);
                int alphaZ = Mathf.FloorToInt(normalizedZ * alphaMapHeight);

                // Paint logic (similar to what you had before)
                int radius = Mathf.FloorToInt(pathWidth * 0.5f);
                for (int x = alphaX - radius; x <= alphaX + radius; x++)
                {
                    for (int z = alphaZ - radius; z <= alphaZ + radius; z++)
                    {
                        if (x >= 0 && x < alphaMapWidth && z >= 0 && z < alphaMapHeight)
                        {
                            // Distance from path center
                            float dist = Vector2.Distance(new Vector2(x, z), new Vector2(alphaX, alphaZ));
                            if (dist <= radius)
                            {
                                for (int j = 0; j < terrainData.alphamapLayers; j++)
                                {
                                    alphaMap[z, x, j] = j == pathTextureIndex ? 1.0f : 0.0f;
                                }
                            }
                        }
                    }
                }
            }
        }

        terrainData.SetAlphamaps(0, 0, alphaMap);
    }

    private int GetUniqueTextureIndex(UnityEngine.TerrainData terrainData)
    {
        int textureCount = terrainData.alphamapLayers;
        HashSet<int> usedTextures = new HashSet<int>();

        for (int x = 0; x < terrainData.alphamapWidth; x++)
        {
            for (int z = 0; z < terrainData.alphamapHeight; z++)
            {
                for (int layer = 0; layer < textureCount; layer++)
                {
                    if (terrainData.GetAlphamaps(x, z, 1, 1)[0, 0, layer] > 0.5f)
                    {
                        usedTextures.Add(layer);
                    }
                }
            }
        }

        int randomTextureIndex;
        int maxAttempts = 100;
        for (int i = 0; i < maxAttempts; i++)
        {
            randomTextureIndex = Random.Range(0, textureCount);
            if (!usedTextures.Contains(randomTextureIndex))
            {
                return randomTextureIndex;
            }
        }

        return 0;
    }
}




// using System.Collections.Generic;
// using UnityEditor;
// using UnityEngine;

// namespace CurvedPathGenerator
// {

//     [RequireComponent(typeof(MeshFilter))]
//     [RequireComponent(typeof(MeshRenderer))]
//     [System.Serializable]
//     public class PathGenerator : MonoBehaviour
//     {
//         public bool IsClosed = false;

//         public bool IsLivePath = false;

//         public bool IsShowingIcons = true;

//         public int PathDensity = 5;

//         public int EditMode = 0;

//         public bool CreateMeshFlag = true;

//         public float LineMehsWidth = 0.2f;

//         public float LineOpacity = 0.7f;

//         public float LineSpeed = 10f;

//         public float LineTiling = 20f;

//         public float LineFilling = 1f;

//         public int LineRenderQueue = 2500;

//         public Texture2D LineTexture;

//         public int NodeCount = 6; 
//         public float PathLength = 1024f; 
//         public float AngleVarianceX = 30f; // Random angle variance for X axis
//         public float AngleVarianceY = 30f; // Random angle variance for Y axis
//         public float NoiseScale = 0.1f; 
//         public float HeightNoiseScale = 0.05f;

//         public List<Vector3> PathList = new List<Vector3>();

//         public List<float> PathLengths = new List<float>();

//         [SerializeField]
//         public List<Vector3> NodeList = new List<Vector3>();


//         [SerializeField]
//         public List<Vector3> AngleList = new List<Vector3>();

 
//         public List<Vector3> NodeList_World = new List<Vector3>();


//         public List<Vector3> AngleList_World = new List<Vector3>();

//         private Terrain terrain;

//         // Generates nodes with random heights and angles using Perlin noise
//         public void GenerateRandomNodesAndAngles()
//         {
//             NodeList.Clear();
//             AngleList.Clear();

//             Vector3 lastNode = Vector3.zero;

//             for (int i = 0; i < NodeCount; i++)
//             {
//                 // Generate a random position using Perlin noise for smooth transitions
//                 float randomX = Mathf.PerlinNoise(i * NoiseScale, 0f) * PathLength;
//                 float randomZ = Mathf.PerlinNoise(0f, i * NoiseScale) * PathLength;

//                 // Sample the terrain's height at the random X, Z position
//                 float terrainHeight = SampleTerrainHeight(randomX, randomZ);

//                 Vector3 newNode = new Vector3(randomX, terrainHeight, randomZ);
//                 NodeList.Add(newNode);

//                 // Randomize the angles for both X and Y axes
//                 float randomAngleX = Random.Range(-AngleVarianceX, AngleVarianceX);
//                 float randomAngleY = Random.Range(-AngleVarianceY, AngleVarianceY);
//                 AngleList.Add(new Vector3(randomAngleX, randomAngleY, 0));

//                 lastNode = newNode;
//             }

//             NodeList_World = NodeList; 
//             AngleList_World = AngleList; 
//         }
//         private float SampleTerrainHeight(float x, float z)
//         {
//             if (terrain == null)
//                 return 0f;

//             Vector3 terrainPos = terrain.transform.position;
//             float normalizedX = (x - terrainPos.x) / terrain.terrainData.size.x;
//             float normalizedZ = (z - terrainPos.z) / terrain.terrainData.size.z;

//             return terrain.SampleHeight(new Vector3(x, 0, z)) + terrainPos.y;
//         }

//         public void UpdatePath()
//         {
//             if (PathDensity < 2)
//             {
// #if UNITY_EDITOR
//                 Debug.LogError("Path Density is too small. (must >= 2)");
//                 UnityEditor.EditorApplication.isPlaying = false;
// #elif UNITY_WEBPLAYER
//                 Application.OpenURL("about:blank");
// #else
//                 Application.Quit();
// #endif
//                 return; // Exit if path density is too low
//             }

//             try
//             {
//                 PathList = new List<Vector3>();
//                 PathLengths = new List<float>();

//                 for (int i = 0; i < NodeList_World.Count; i++)
//                 {
//                     Vector3 startPoint = NodeList_World[i];
//                     Vector3 middlePoint = new Vector3();
//                     Vector3 endPoint = new Vector3();
//                     if (i == NodeList_World.Count - 1)
//                     {
//                         if (IsClosed)
//                         {
//                             middlePoint = AngleList_World[i];
//                             endPoint = NodeList_World[0];
//                         }
//                         else
//                         {
//                             break;
//                         }
//                     }
//                     else
//                     {
//                         middlePoint = AngleList_World[i];
//                         endPoint = NodeList_World[i + 1];
//                     }

//                     // Generate the Bezier curve with path density
//                     for (int j = 0; j < PathDensity; j++)
//                     {
//                         float t = (float)j / PathDensity;
//                         Vector3 curve = (1f - t) * (1f - t) * startPoint +
//                                         2 * (1f - t) * t * middlePoint +
//                                         t * t * endPoint;
//                         PathList.Add(curve);

//                         if (PathList.Count > 1)
//                         {
//                             float length = (PathList[PathList.Count - 2] - curve).magnitude;
//                             PathLengths.Add(PathLengths.Count == 0 ? length : PathLengths[PathLengths.Count - 1] + length);
//                         }
//                     }
//                 }

//                 if (IsClosed)
//                     PathList.Add(NodeList_World[0]);
//                 else
//                     PathList.Add(NodeList_World[NodeList_World.Count - 1]);

//                 CreateMesh(PathList);
//             }
//             catch (System.Exception e)
//             {
//                 Debug.LogError($"Path generation failed: {e}");
//             }
//         }


//         public float GetLength()
//         {
//             if ( PathLengths != null || PathLengths.Count > 0 )
//             {
//                 return PathLengths[PathLengths.Count - 1];
//             }
//             else
//             {
//                 return 0;
//             }
//         }


//         private void Update()
//         {
//             if ( IsLivePath )
//             {
//                 UpdatePath();
//             }
//         }


//         private void CreateMesh(List<Vector3> pathVec)
//         {
//             if ( !CreateMeshFlag )
//             {
//                 return;
//             }

//             Quaternion rotation = transform.rotation;
//             Matrix4x4 m_reverse = Matrix4x4.Rotate(Quaternion.Inverse(rotation));
//             int verNum = 2 * pathVec.Count;
//             int triNum = 6 * ( pathVec.Count - 1 );
//             Vector3[] vertices = new Vector3[verNum];
//             int[] triangles = new int[triNum];
//             Vector2[] uvs = new Vector2[verNum];

//             float MaxLength = 0, currentLength = 0;
//             for ( int i = 1 ; i < pathVec.Count ; i++ )
//             {
//                 MaxLength += ( pathVec[i] - pathVec[i - 1] ).magnitude;
//             }

//             for ( int i = 0 ; i < pathVec.Count - 1 ; i++ )
//             {
//                 Vector3 dir = ( pathVec[i + 1] - pathVec[i] ).normalized;
//                 Vector3 new_dir1 = new Vector3(dir.z, 0, -dir.x);
//                 Vector3 new_dir2 = new Vector3(-dir.z, 0, dir.x);


//                 if ( i == 0 )
//                 {
//                     vertices[2 * i] = ReverseTransformPoint(pathVec[i] + ( new_dir1 * ( LineMehsWidth / 2 ) ), m_reverse);
//                     vertices[2 * i + 1] = ReverseTransformPoint(pathVec[i] + ( new_dir2 * ( LineMehsWidth / 2 ) ), m_reverse);
//                     uvs[2 * i] = new Vector2(0.5f, -0.5f);
//                     uvs[2 * i + 1] = new Vector2(-0.5f, -0.5f);
//                 }

//                 else
//                 {
//                     currentLength += ( pathVec[i] - pathVec[i - 1] ).magnitude;

//                     vertices[2 * i] = ReverseTransformPoint(pathVec[i] + ( new_dir1 * ( LineMehsWidth / 2 ) ), m_reverse);
//                     vertices[2 * i + 1] = ReverseTransformPoint(pathVec[i] + ( new_dir2 * ( LineMehsWidth / 2 ) ), m_reverse);
//                     uvs[2 * i] = new Vector2(0.5f, -0.5f + ( currentLength ) / ( MaxLength ));
//                     uvs[2 * i + 1] = new Vector2(-0.5f, -0.5f + ( currentLength ) / ( MaxLength ));
//                 }


//                 if ( i == pathVec.Count - 2 )
//                 {
//                     vertices[2 * i + 2] = ReverseTransformPoint(pathVec[i + 1] + ( new_dir1 * ( LineMehsWidth / 2 ) ), m_reverse);
//                     vertices[2 * i + 3] = ReverseTransformPoint(pathVec[i + 1] + ( new_dir2 * ( LineMehsWidth / 2 ) ), m_reverse);
//                     uvs[2 * i + 2] = new Vector2(0.5f, 0.5f);
//                     uvs[2 * i + 3] = new Vector2(-0.5f, 0.5f);
//                 }
//             }


//             for ( int i = 0 ; i < pathVec.Count - 1 ; i++ )
//             {
//                 triangles[6 * i] = 2 * i + 3;
//                 triangles[6 * i + 1] = 2 * i + 2;
//                 triangles[6 * i + 2] = 2 * i;
//                 triangles[6 * i + 3] = 2 * i + 3;
//                 triangles[6 * i + 4] = 2 * i;
//                 triangles[6 * i + 5] = 2 * i + 1;
//             }


//             MeshFilter PathMesh = transform.GetComponent<MeshFilter>();
//             Mesh newMesh = new Mesh();
//             newMesh.vertices = vertices;
//             newMesh.triangles = triangles;
//             newMesh.uv = uvs;
//             newMesh.RecalculateBounds();
//             newMesh.RecalculateNormals();
//             PathMesh.mesh = newMesh;
//         }


//         private void OnDrawGizmosSelected()
//         {
// #if UNITY_EDITOR
//             Tools.hidden = ( EditMode != 0 );
//             if ( IsShowingIcons )
//             {
//                 Gizmos.DrawIcon(this.transform.position, "PathGenerator/PG_Anchor.png", true);
//                 if ( NodeList_World != null && NodeList_World.Count > 0 )
//                 {
//                     for ( int i = 0 ; i < NodeList_World.Count ; i++ )
//                     {
//                         if ( i == 0 )
//                         {
//                             Gizmos.DrawIcon(NodeList_World[i], "PathGenerator/PG_Start.png", ( EditMode != 0 ));
//                         }
//                         else if ( !IsClosed && i == NodeList_World.Count - 1 )
//                         {
//                             Gizmos.DrawIcon(NodeList_World[i], "PathGenerator/PG_End.png", ( EditMode != 0 ));
//                         }
//                         else
//                         {
//                             Gizmos.DrawIcon(NodeList_World[i], "PathGenerator/PG_Node.png", ( EditMode != 0 ));
//                         }
//                     }
//                 }

//                 if ( AngleList_World != null && AngleList_World.Count > 0 )
//                 {
//                     for ( int i = 0 ; i < AngleList_World.Count ; i++ )
//                     {
//                         Gizmos.DrawIcon(AngleList_World[i], "PathGenerator/PG_Handler.png", ( EditMode != 0 ));
//                     }
//                 }
//             }
// #endif
//         }


//         public void ResetTools()
//         {
// #if UNITY_EDITOR
//             Tools.hidden = false;
// #endif
//         }
//         private Vector3 ReverseTransformPoint(Vector3 points, Matrix4x4 m_reverse)
//         {
//             Vector3 result = points;

//             result -= transform.position;                   
//             result = m_reverse.MultiplyPoint3x4(result);    
//             result = new Vector3(                           
//                 result.x / transform.lossyScale.x,
//                 result.y / transform.lossyScale.y,
//                 result.z / transform.lossyScale.z
//             );
//             return result;
//         }
//     }
// }
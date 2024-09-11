
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CurvedPathGenerator
{

    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [System.Serializable]
    public class PathGenerator : MonoBehaviour
    {
        public bool IsClosed = false;

        public bool IsLivePath = false;

        public bool IsShowingIcons = true;

        public int PathDensity = 5;

        public int EditMode = 0;

        public bool CreateMeshFlag = true;

        public float LineMehsWidth = 0.2f;

        public float LineOpacity = 0.7f;

        public float LineSpeed = 10f;

        public float LineTiling = 20f;

        public float LineFilling = 1f;

        public int LineRenderQueue = 2500;

        public Texture2D LineTexture;

        public int NodeCount = 6; 
        public float PathLength = 1024f; 
        public float AngleVarianceX = 30f; // Random angle variance for X axis
        public float AngleVarianceY = 30f; // Random angle variance for Y axis
        public float NoiseScale = 0.1f; 
        public float HeightNoiseScale = 0.05f;

        public List<Vector3> PathList = new List<Vector3>();

        public List<float> PathLengths = new List<float>();

        [SerializeField]
        public List<Vector3> NodeList = new List<Vector3>();


        [SerializeField]
        public List<Vector3> AngleList = new List<Vector3>();

 
        public List<Vector3> NodeList_World = new List<Vector3>();


        public List<Vector3> AngleList_World = new List<Vector3>();

        private Terrain terrain;

        // Generates nodes with random heights and angles using Perlin noise
        public void GenerateRandomNodesAndAngles()
        {
            NodeList.Clear();
            AngleList.Clear();

            Vector3 lastNode = Vector3.zero;

            for (int i = 0; i < NodeCount; i++)
            {
                // Generate a random position using Perlin noise for smooth transitions
                float randomX = Mathf.PerlinNoise(i * NoiseScale, 0f) * PathLength;
                float randomZ = Mathf.PerlinNoise(0f, i * NoiseScale) * PathLength;

                // Sample the terrain's height at the random X, Z position
                float terrainHeight = SampleTerrainHeight(randomX, randomZ);

                Vector3 newNode = new Vector3(randomX, terrainHeight, randomZ);
                NodeList.Add(newNode);

                // Randomize the angles for both X and Y axes
                float randomAngleX = Random.Range(-AngleVarianceX, AngleVarianceX);
                float randomAngleY = Random.Range(-AngleVarianceY, AngleVarianceY);
                AngleList.Add(new Vector3(randomAngleX, randomAngleY, 0));

                lastNode = newNode;
            }

            NodeList_World = NodeList; 
            AngleList_World = AngleList; 
        }
        private float SampleTerrainHeight(float x, float z)
        {
            if (terrain == null)
                return 0f;

            Vector3 terrainPos = terrain.transform.position;
            float normalizedX = (x - terrainPos.x) / terrain.terrainData.size.x;
            float normalizedZ = (z - terrainPos.z) / terrain.terrainData.size.z;

            return terrain.SampleHeight(new Vector3(x, 0, z)) + terrainPos.y;
        }

        public void UpdatePath()
        {
            if (PathDensity < 2)
            {
#if UNITY_EDITOR
                Debug.LogError("Path Density is too small. (must >= 2)");
                UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_WEBPLAYER
                Application.OpenURL("about:blank");
#else
                Application.Quit();
#endif
                return; // Exit if path density is too low
            }

            try
            {
                PathList = new List<Vector3>();
                PathLengths = new List<float>();

                for (int i = 0; i < NodeList_World.Count; i++)
                {
                    Vector3 startPoint = NodeList_World[i];
                    Vector3 middlePoint = new Vector3();
                    Vector3 endPoint = new Vector3();
                    if (i == NodeList_World.Count - 1)
                    {
                        if (IsClosed)
                        {
                            middlePoint = AngleList_World[i];
                            endPoint = NodeList_World[0];
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        middlePoint = AngleList_World[i];
                        endPoint = NodeList_World[i + 1];
                    }

                    // Generate the Bezier curve with path density
                    for (int j = 0; j < PathDensity; j++)
                    {
                        float t = (float)j / PathDensity;
                        Vector3 curve = (1f - t) * (1f - t) * startPoint +
                                        2 * (1f - t) * t * middlePoint +
                                        t * t * endPoint;
                        PathList.Add(curve);

                        if (PathList.Count > 1)
                        {
                            float length = (PathList[PathList.Count - 2] - curve).magnitude;
                            PathLengths.Add(PathLengths.Count == 0 ? length : PathLengths[PathLengths.Count - 1] + length);
                        }
                    }
                }

                if (IsClosed)
                    PathList.Add(NodeList_World[0]);
                else
                    PathList.Add(NodeList_World[NodeList_World.Count - 1]);

                CreateMesh(PathList);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Path generation failed: {e}");
            }
        }


        public float GetLength()
        {
            if ( PathLengths != null || PathLengths.Count > 0 )
            {
                return PathLengths[PathLengths.Count - 1];
            }
            else
            {
                return 0;
            }
        }


        private void Update()
        {
            if ( IsLivePath )
            {
                UpdatePath();
            }
        }


        private void CreateMesh(List<Vector3> pathVec)
        {
            if ( !CreateMeshFlag )
            {
                return;
            }

            Quaternion rotation = transform.rotation;
            Matrix4x4 m_reverse = Matrix4x4.Rotate(Quaternion.Inverse(rotation));
            int verNum = 2 * pathVec.Count;
            int triNum = 6 * ( pathVec.Count - 1 );
            Vector3[] vertices = new Vector3[verNum];
            int[] triangles = new int[triNum];
            Vector2[] uvs = new Vector2[verNum];

            float MaxLength = 0, currentLength = 0;
            for ( int i = 1 ; i < pathVec.Count ; i++ )
            {
                MaxLength += ( pathVec[i] - pathVec[i - 1] ).magnitude;
            }

            for ( int i = 0 ; i < pathVec.Count - 1 ; i++ )
            {
                Vector3 dir = ( pathVec[i + 1] - pathVec[i] ).normalized;
                Vector3 new_dir1 = new Vector3(dir.z, 0, -dir.x);
                Vector3 new_dir2 = new Vector3(-dir.z, 0, dir.x);


                if ( i == 0 )
                {
                    vertices[2 * i] = ReverseTransformPoint(pathVec[i] + ( new_dir1 * ( LineMehsWidth / 2 ) ), m_reverse);
                    vertices[2 * i + 1] = ReverseTransformPoint(pathVec[i] + ( new_dir2 * ( LineMehsWidth / 2 ) ), m_reverse);
                    uvs[2 * i] = new Vector2(0.5f, -0.5f);
                    uvs[2 * i + 1] = new Vector2(-0.5f, -0.5f);
                }

                else
                {
                    currentLength += ( pathVec[i] - pathVec[i - 1] ).magnitude;

                    vertices[2 * i] = ReverseTransformPoint(pathVec[i] + ( new_dir1 * ( LineMehsWidth / 2 ) ), m_reverse);
                    vertices[2 * i + 1] = ReverseTransformPoint(pathVec[i] + ( new_dir2 * ( LineMehsWidth / 2 ) ), m_reverse);
                    uvs[2 * i] = new Vector2(0.5f, -0.5f + ( currentLength ) / ( MaxLength ));
                    uvs[2 * i + 1] = new Vector2(-0.5f, -0.5f + ( currentLength ) / ( MaxLength ));
                }


                if ( i == pathVec.Count - 2 )
                {
                    vertices[2 * i + 2] = ReverseTransformPoint(pathVec[i + 1] + ( new_dir1 * ( LineMehsWidth / 2 ) ), m_reverse);
                    vertices[2 * i + 3] = ReverseTransformPoint(pathVec[i + 1] + ( new_dir2 * ( LineMehsWidth / 2 ) ), m_reverse);
                    uvs[2 * i + 2] = new Vector2(0.5f, 0.5f);
                    uvs[2 * i + 3] = new Vector2(-0.5f, 0.5f);
                }
            }


            for ( int i = 0 ; i < pathVec.Count - 1 ; i++ )
            {
                triangles[6 * i] = 2 * i + 3;
                triangles[6 * i + 1] = 2 * i + 2;
                triangles[6 * i + 2] = 2 * i;
                triangles[6 * i + 3] = 2 * i + 3;
                triangles[6 * i + 4] = 2 * i;
                triangles[6 * i + 5] = 2 * i + 1;
            }


            MeshFilter PathMesh = transform.GetComponent<MeshFilter>();
            Mesh newMesh = new Mesh();
            newMesh.vertices = vertices;
            newMesh.triangles = triangles;
            newMesh.uv = uvs;
            newMesh.RecalculateBounds();
            newMesh.RecalculateNormals();
            PathMesh.mesh = newMesh;
        }


        private void OnDrawGizmosSelected()
        {
#if UNITY_EDITOR
            Tools.hidden = ( EditMode != 0 );
            if ( IsShowingIcons )
            {
                Gizmos.DrawIcon(this.transform.position, "PathGenerator/PG_Anchor.png", true);
                if ( NodeList_World != null && NodeList_World.Count > 0 )
                {
                    for ( int i = 0 ; i < NodeList_World.Count ; i++ )
                    {
                        if ( i == 0 )
                        {
                            Gizmos.DrawIcon(NodeList_World[i], "PathGenerator/PG_Start.png", ( EditMode != 0 ));
                        }
                        else if ( !IsClosed && i == NodeList_World.Count - 1 )
                        {
                            Gizmos.DrawIcon(NodeList_World[i], "PathGenerator/PG_End.png", ( EditMode != 0 ));
                        }
                        else
                        {
                            Gizmos.DrawIcon(NodeList_World[i], "PathGenerator/PG_Node.png", ( EditMode != 0 ));
                        }
                    }
                }

                if ( AngleList_World != null && AngleList_World.Count > 0 )
                {
                    for ( int i = 0 ; i < AngleList_World.Count ; i++ )
                    {
                        Gizmos.DrawIcon(AngleList_World[i], "PathGenerator/PG_Handler.png", ( EditMode != 0 ));
                    }
                }
            }
#endif
        }


        public void ResetTools()
        {
#if UNITY_EDITOR
            Tools.hidden = false;
#endif
        }
        private Vector3 ReverseTransformPoint(Vector3 points, Matrix4x4 m_reverse)
        {
            Vector3 result = points;

            result -= transform.position;                   
            result = m_reverse.MultiplyPoint3x4(result);    
            result = new Vector3(                           
                result.x / transform.lossyScale.x,
                result.y / transform.lossyScale.y,
                result.z / transform.lossyScale.z
            );
            return result;
        }
    }
}
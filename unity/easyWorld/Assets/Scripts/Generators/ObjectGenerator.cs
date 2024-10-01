using Assets.Scripts.MapGenerator.Maps;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.MapGenerator.Generators
{
    public class ObjectGenerator : Generator
    {
        private Dictionary<string, GameObject> objectMapping;
        private List<GameObject> objectList;
        public float waterLevel = 4f;

        private void Start()
        {
            // Initialize object mappings
            objectMapping = new Dictionary<string, GameObject>(){
                {"Brick House", Resources.Load<GameObject>("Brick_House/Prefabs/Brick_House_2.79")},
                {"Ferris Wheel", Resources.Load<GameObject>("Low Poly Houses Free Pack/Prefabs/Lunapark/ferris wheel")},
                {"Small House", Resources.Load<GameObject>("Low Poly Houses Free Pack/Prefabs/Houses with environment/cute house with environment")}
            };
            objectList = new List<GameObject>();
        }

        public override IEnumerator Generate(WorldInfo worldInfo, int terrainIndex)
        {
            foreach (var objData in worldInfo.objectList)
            {
                GenerateObject(objData, terrainIndex);
            }

            yield return null;
        }

        public override void Clear()
        {
            foreach (var obj in objectList)
            {
                Destroy(obj);
            }
            objectList.Clear();
        }

        private void GenerateObject(ObjectGeneratorData data, int terrainIndex)
        {
            if (data == null || string.IsNullOrEmpty(data.name))
            {
                Debug.LogWarning("Invalid ObjectGeneratorData.");
                return;
            }

            if (!objectMapping.ContainsKey(data.name))
            {
                Debug.LogWarning($"Object '{data.name}' not found in object mapping.");
                return;
            }

            GameObject go = Instantiate(objectMapping[data.name]);

            go.transform.localScale = new Vector3(data.scale, data.scale, data.scale);

            Terrain terrain = TerrainGenerator.GetTerrainByIndexOrCreate(terrainIndex, 1024, 200, 1024); 
            if (terrain == null)
            {
                Debug.LogError($"Failed to get or create terrain at index {terrainIndex}.");
                return;
            }

            float terrainHeightAtPosition = terrain.SampleHeight(new Vector3(data.x, 0, data.y));

            if (terrainHeightAtPosition < waterLevel)
            {
                Debug.LogWarning($"Skipping {data.name} at ({data.x}, {data.y}) because it's in water.");
                return;
            }

            var renderer = go.GetComponent<MeshRenderer>();
            var length = renderer.bounds.size.x;
            var width = renderer.bounds.size.z;

            var terrainHeights = terrain.terrainData.GetHeights((int)(data.x - length / 2), (int)(data.y - width / 2), (int)length + 4, (int)width + 4);
            var maxHeight = terrainHeights.Cast<float>().Max();

            for (int i = 0; i < terrainHeights.GetLength(0); i++)
            {
                for (int j = 0; j < terrainHeights.GetLength(1); j++)
                {
                    terrainHeights[i, j] = maxHeight;
                }
            }
            terrain.terrainData.SetHeights((int)(data.x - length / 2), (int)(data.y - width / 2), terrainHeights);

            go.transform.position = new Vector3(
                data.x,
                terrain.SampleHeight(new Vector3(data.x, 0, data.y)),
                data.y
            );

            go.transform.rotation = Quaternion.Euler(new Vector3(data.Rx, data.Ry, data.Rz));
            objectList.Add(go);

            Debug.Log($"Added {data.name} to terrain {terrainIndex} at ({data.x}, {data.y}) with height {terrainHeightAtPosition}.");
        }
    }
}
using Assets.Scripts.MapGenerator.Maps;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using Dummiesman;
using UnityEngine;

namespace Assets.Scripts.MapGenerator.Generators
{
    public class GeneratedObjectGenerator : Generator
    {
        private List<GameObject> objectList;
        public float waterLevel = 4f;

        private void Start()
        {
            // Initialize object list
            objectList = new List<GameObject>();
        }

        public override IEnumerator Generate(WorldInfo worldInfo, int terrainIndex)
        {
            foreach (var objData in worldInfo.generatedObjectList)
            {
                StartCoroutine(GenerateObject(objData, terrainIndex));
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

        private IEnumerator GenerateObject(GeneratedObjectGeneratorData data, int terrainIndex)
        {
            if (data == null || string.IsNullOrEmpty(data.file_name))
            {
                Debug.LogWarning("Invalid ObjectGeneratorData.");
                yield return null;
            }

            //Call shape-e here with the object description and file name
            yield return StartCoroutine(CreateObject(data));

            Debug.Log("FINISHED GENERATING OBJECT");
            var objLoader = new OBJLoader();
            string baseFileName = $"Assets/Resources/GeneratedObjects/{data.file_name}";
            GameObject go = null;
            try{
                go = objLoader.Load(baseFileName); 
            }catch(Exception E){
                Debug.Log(E);
            }
            
            if(go == null)
                Debug.Log("ERROR: OBJECT IS NULL!!!");

            
            go.transform.localScale = new Vector3(data.scale, data.scale, data.scale);

            Terrain terrain = TerrainGenerator.GetTerrainByIndexOrCreate(terrainIndex, 1024, 200, 1024); 
            if (terrain == null)
            {
                Debug.LogError($"Failed to get or create terrain at index {terrainIndex}.");
                yield return null;
            }

            float terrainHeightAtPosition = terrain.SampleHeight(new Vector3(data.x, 0, data.y));

            if (terrainHeightAtPosition < waterLevel)
            {
                Debug.LogWarning($"Skipping {data.file_name} at ({data.x}, {data.y}) because it's in water.");
                yield return null;
            }

            UpdateTerrainHeightsForObject(terrain, go, data);

            PlaceObjectInWorld(go, data, terrain);

            objectList.Add(go);

            Debug.Log($"Added {data.file_name} to terrain {terrainIndex} at ({data.x}, {data.y}) with height {terrainHeightAtPosition}.");
            
        }

        private IEnumerator CreateObject(GeneratedObjectGeneratorData data){
            
            string baseFileName = $"Assets/Resources/GeneratedObjects/{data.file_name}";
            Debug.Log($"Generating at {data.file_name}");
            
            yield return new WaitForSeconds(4);

            Debug.Log("Object Created!");

        }

        /**
         * Updates the terrain heights so that the object can be placed on a flat ground.
         * Every needed point on the terrain gets moved up, includes a 4 unit boundary around objects.
         */
        private void UpdateTerrainHeightsForObject(Terrain terrain, GameObject go, GeneratedObjectGeneratorData data){
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
        }

        /**
         * Places the object in the world.
         */
        private void PlaceObjectInWorld(GameObject go, GeneratedObjectGeneratorData data, Terrain terrain){
            go.transform.position = new Vector3(
                data.x,
                terrain.SampleHeight(new Vector3(data.x, 0, data.y)),
                data.y
            );

            go.transform.rotation = Quaternion.Euler(new Vector3(data.Rx, data.Ry, data.Rz));

        }
    }
}

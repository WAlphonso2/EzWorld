using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace DirectlyGenerator
{
    public class DirectlyGenerator : MonoBehaviour
    {
        public List<Generator> generators;


        [Header("World Settings")]
        public WorldInfo worldInfo; // Make WorldInfo editable in Inspector
        public int terrainIndex;
        public void ClearCurrentWorld()
        {
            Debug.Log("Clearing current world");
            generators.ForEach(g => g.Clear());
        }

        public void OnGenerateBased_onWorldInfo()
        {
            ClearCurrentWorld();
            GenerateNewWorld(worldInfo);
        }

        public void GenerateNewWorld(WorldInfo worldInfo)
        {

            foreach (Generator g in generators)
            {
                Debug.Log($"Using {g.GetType().Name}");

                StartCoroutine(g.Generate(worldInfo, terrainIndex));
            }

            Debug.Log("Started all generators successfully");
        }

        public void OnApplicationQuit()
        {
            ClearCurrentWorld();
        }
    }
}
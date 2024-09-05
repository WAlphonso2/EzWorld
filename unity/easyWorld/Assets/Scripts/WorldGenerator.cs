using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scenes.Patrick_Terrain
{
    public class WorldGenerator : MonoBehaviour
    {
        public List<Generator> generators;

        public IEnumerator ClearCurrentWorld()
        {
            generators.ForEach(g => g.Clear());
            yield return null;
        }

        public void GenerateNewWorld(WorldInfo worldInfo)
        {
            // tell all generators to generate their parts of the world which will run in parallel
            foreach (Generator g in generators)
            {
                StartCoroutine(g.Generate(worldInfo));
            }
        }
    }
}

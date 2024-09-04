using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scenes.Patrick_Terrain
{
    public class WorldGenerator : MonoBehaviour
    {
        public List<Generator> generators;

        public void GenerateNewWorld(WorldInfo worldInfo)
        {
            Debug.Log(worldInfo);

            // tell all generators to clear up anything from previous world
            generators.ForEach(g => g.Clear());

            // tell all generators to generate their worlds which will run in parallel
            foreach (Generator g in generators)
            {
                StartCoroutine(g.Generate(worldInfo));
            }
        }
    }
}

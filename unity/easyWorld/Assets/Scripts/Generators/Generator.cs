using System.Collections;
using UnityEngine;

public abstract class Generator : MonoBehaviour
{
    public abstract IEnumerator Generate(WorldInfo worldInfo, int terrainIndex = 0);

    public abstract void Clear();
}

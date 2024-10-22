using System.Collections;
using UnityEngine;

public abstract class Generator : MonoBehaviour
{
    public abstract IEnumerator Generate(WorldInfo worldInfo, int terrainIndex);

    public abstract void Clear();
}

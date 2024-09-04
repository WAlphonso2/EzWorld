using System.Collections;
using UnityEngine;

public abstract class Generator : MonoBehaviour
{
    public abstract IEnumerator Generate(WorldInfo worldInfo);

    public abstract void Clear();
}

/*
 * Define parameters to the world generation function.
 * Each parameter should have a description stating how the ai
 * model should handle assigning it a value
 */
[System.Serializable]
public struct WorldInfo
{
    /*
     * Terrain Material must match a value from the TerrainMaterial enum reprsented as an integer
     */
    public TerrainMaterial TerrainMaterial;

    /*
     * Cloud Density must be a value in the range [0,1]
     */
    public float CloudDensity;

    public override readonly string ToString()
    {
        return $"\nTerrainMaterial: {TerrainMaterial}" +
            $"\nCloudDensity: {CloudDensity}";
    }
}

public enum TerrainMaterial : int
{
    None = 0,
    Grass = 1,
    Dirt = 2,
    Snow = 3,
    Sand = 4,
}
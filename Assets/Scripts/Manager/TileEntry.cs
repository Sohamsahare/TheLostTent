using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public struct TileEntry
{
    public SpawnType spawnType;
    public Color color;
    public GameObject[] prefabs;
    public TileBase[] tiles;
    public int elevation;
    public string name;
}

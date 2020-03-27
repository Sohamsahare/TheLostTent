using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelData : MonoBehaviour
{
    public int numLayers;
    public Texture2D[] levelmaps;
    public Tilemap[] tilemaps;
    public LevelData(int numLayers, Texture2D[] levelmaps, Tilemap[] tilemaps)
    {
        this.numLayers = numLayers;
        this.levelmaps = levelmaps;
        this.tilemaps = tilemaps;
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
namespace TheLostTent
{
    public class TileManager : MonoBehaviour
    {
        public bool isUsingTilemaps = false;
        [Range(1.01f, 2)]
        public float elevationAdjuster = 1.5f;
        public Vector2 rowOffset;
        public Vector2 colOffset;
        private float elevationOffset
        {
            get
            {
                return colOffset.y / elevationAdjuster;
            }
        }
        public TileEntry[] tileEntries;
        private LevelData levelData;
        [SerializeField]
        private Vector2 startPosition = Vector2.zero;
        Dictionary<Color, TileEntry> colorToTileDictionary;
        private void Start()
        {
            BuildLevel();
        }

        public void BuildLevel()
        {
            levelData = GetComponent<LevelData>();
            colorToTileDictionary = new Dictionary<Color, TileEntry>();

            foreach (TileEntry tileEntry in tileEntries)
            {
                colorToTileDictionary.Add(tileEntry.color, tileEntry);
                Debug.Log("Adding " + tileEntry.color + " to dictionary");
            }

            for (int current = 0; current < levelData.numLayers; current++)
            {
                // Vector2 startPosition = getCurrentPosition(current);
                SpawnTiles(current, this.startPosition);
            }
        }

        private Vector2 getCurrentPosition(int current)
        {
            Vector2 position = new Vector2();
            int levelMapPPU = levelData.levelMapPPU;
            switch (current)
            {
                // top left
                case 0:
                    position = startPosition + new Vector2(1, -1) * levelMapPPU;
                    break;
                // top middle
                case 1:
                    position = startPosition + Vector2.up * levelMapPPU;
                    break;
                // top right
                case 2:
                    position = startPosition + Vector2.one * levelMapPPU;
                    break;
                // middle left
                case 3:
                    position = startPosition + Vector2.left * levelMapPPU;
                    break;
                case 5:
                    position = startPosition + Vector2.right * levelMapPPU;
                    break;
                case 6:
                    position = startPosition + Vector2.one * -1 * levelMapPPU;
                    break;
                case 7:
                    position = startPosition + Vector2.down * -1 * levelMapPPU;
                    break;
                case 8:
                    position = startPosition + new Vector2(1, -1) * levelMapPPU;
                    break;
                // middle case
                default:
                    position = startPosition;
                    break;
            }
            return position;
        }

        Vector2 getNextTilePosition(int w, int h, float elevation)
        {
            Vector2 rowStart = startPosition + colOffset * h;
            Vector2 rowTile = rowStart + w * rowOffset;
            rowTile.y += elevation * elevationOffset;
            return rowTile;
        }

        void SpawnTiles(int current, Vector2 startPosition)
        {
            var levelMap = levelData.levelmaps[current];
            if (isUsingTilemaps)
            {
                // placing tiles on map
                var tilemap = levelData.tilemaps[current];
                PlaceTiles(levelMap, startPosition);
            }
            else
            {
                startPosition = new Vector2(-levelMap.width / 2, 0);
                // spawning prefabs
                var tilemap = new GameObject("map-" + current);
                tilemap.transform.SetParent(transform);
                InstantiateTiles(current, levelMap, tilemap);
            }
        }

        private void PlaceColliders(Texture2D levelMap)
        {
            throw new NotImplementedException();
        }

        private void PlaceTiles(Texture2D levelMap, Vector2 startPosition)
        {
            // for each pixel in map
            for (int h = 0; h < levelMap.height; h++)
            {
                for (int w = 0; w < levelMap.width; w++)
                {
                    // get pixel value
                    Color color = levelMap.GetPixel(w, h);

                    // get position of spawn
                    // check if in dict
                    if (colorToTileDictionary.ContainsKey(color))
                    {
                        TileEntry tileEntry = colorToTileDictionary[color];
                        var tilemap = levelData.tilemaps[tileEntry.elevation];
                        if (tileEntry.elevation != 0)
                        {
                            Debug.Log(JsonUtility.ToJson(tileEntry, true));
                        }
                        Vector3Int position = new Vector3Int(w + (int)startPosition.x, h + (int)startPosition.y, tileEntry.elevation);
                        TileBase tile = null;
                        switch (tileEntry.spawnType)
                        {
                            case SpawnType.Single:
                                tile = tileEntry.tiles[tileEntry.tiles.Length - 1];
                                break;
                            case SpawnType.Random:
                                tile = tileEntry.tiles[UnityEngine.Random.Range(0, tileEntry.tiles.Length)];
                                break;
                            default:
                                tile = null;
                                break;
                        }
                        if (tile == null)
                        {
                            Debug.Log("Tile was null at cell position (" + w + "," + h + ") ->");
                            Debug.Log(tileEntry);
                        }
                        // place tile in respective position
                        tilemap.SetTile(position, tile);
                    }
                    else
                    {
                        Debug.Log("Doesn't contain color -> " + color);
                    }
                }
            }
            // PlaceColliders(levelMap);
        }

        private void InstantiateTiles(int current, Texture2D levelMap, GameObject tilemap)
        {
            // for each pixel in map
            for (int h = 0; h < levelMap.height; h++)
            {
                for (int w = 0; w < levelMap.width; w++)
                {
                    // get pixel value
                    Color color = levelMap.GetPixel(w, h);

                    // get position of spawn
                    // check if in dict
                    if (colorToTileDictionary.ContainsKey(color))
                    {
                        TileEntry tileEntry = colorToTileDictionary[color];
                        Vector2 position = getNextTilePosition(w, h, tileEntry.elevation);
                        GameObject tile = null;
                        switch (tileEntry.spawnType)
                        {
                            case SpawnType.Single:
                                // set tile
                                tile = tileEntry.prefabs[tileEntry.prefabs.Length - 1];
                                break;
                            case SpawnType.Random:
                                // chose random and set tile
                                tile = tileEntry.prefabs[UnityEngine.Random.Range(0, tileEntry.prefabs.Length)];
                                break;
                            default:
                                tile = null;
                                break;
                        }
                        if (tile == null)
                        {
                            Debug.Log("Tile was null at cell position (" + w + "," + h + ") ->");
                            Debug.Log(tileEntry);
                        }
                        // place tile in respective position
                        Instantiate(tile, position, Quaternion.identity);
                    }
                    else
                    {
                        if (color != Color.white)
                        {
                            Debug.Log("Doesn't contain color -> " + color + " for current " + current);
                        }
                    }
                }
            }
        }
    }
}
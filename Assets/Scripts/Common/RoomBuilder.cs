using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomBuilder : MonoBehaviour
{
    public bool isTerminalRoom = false;
    // TODO: should be in tile class
    public Vector2 tileRowOffset = new Vector2(.44f, -.25f);
    public Vector2 tileColumnOffset = new Vector2(.432f, .25f);

    [Range(0.01f, 2)]
    public float elevationOffset = .24f;
    public Texture2D roomLayout;
    public Color[,] layoutAsColors;
    public string Name { get; private set; }
    public Vector2 centerPosition { get; private set; }
    // in case this room was generated linked to other room
    public Transform roomsParent;
    public int downSizeBy = 2;
    // then will not spawn a collider in this direction 
    private RoomManager roomManager;
    private Vector3 startPosition;
    private Vector3Int roomDimensions;
    [SerializeField]
    // bright yellow
    private Color walkableColor = new Color(1, 1, 0, 1);
    public Vector2Int gridPosition { private set; get; }
    public bool isFirst = false;

    private void Awake()
    {
        roomManager = GameObject.FindGameObjectWithTag("RoomManager").GetComponent<RoomManager>();
    }

    private void Start()
    {
        // delay of 0.1 sec
        // so that we can see the map building
        // also roommanager start  function should get over by then
        // Invoke("BuildRoom", 0.05f);
        BuildRoom();
    }

    private void BuildRoom()
    {
        startPosition = transform.position;
        Name = transform.name.Split('(')[0];
        roomDimensions = new Vector3Int(roomLayout.width, roomLayout.height, 0);
        layoutAsColors = new Color[roomLayout.width / downSizeBy, roomLayout.height / downSizeBy];
        ReadLayout();
        InstantiateTiles();
    }

    private void ReadLayout()
    {
        // layoutAsColors = new Color[roomDimensions.x, roomDimensions.y];
        Color[] colors = roomLayout.GetPixels(0, 0, roomDimensions.x, roomDimensions.y);

        for (int index = 0, col = 0, row = 0; index < colors.Length; index += downSizeBy)
        {
            if (index != 0 && ((index / downSizeBy) % roomDimensions.x == 0))
            {
                // start new col
                // Debug.Log($"Incrementing col to {col} at index {index}");
                col++;
            }
            row = (index / downSizeBy) % roomDimensions.y;
            try
            {
                layoutAsColors[row, col] = colors[index];
            }
            catch (IndexOutOfRangeException)
            {
                // Debug.LogWarning($"Assinging color {colors[index]} at ({row},{col}) with size ({layoutAsColors.GetLength(0)},{layoutAsColors.GetLength(1)}) to " + transform.name);
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
            }
        }
    }

    Vector2 getNextTilePosition(int w, int h, float elevation)
    {
        Vector2 rowStart = (Vector2)startPosition + tileColumnOffset * h;
        Vector2 rowTile = rowStart + w * tileRowOffset;
        rowTile.y += elevation * elevationOffset;
        return rowTile;
    }

    public void SetGridPosition(Vector2Int gridPosition)
    {
        this.gridPosition = gridPosition;
    }

    private void InstantiateTiles()
    {
        // for each pixel in map
        for (int y = 0; y < layoutAsColors.GetLength(1); y++)
        {
            for (int x = 0; x < layoutAsColors.GetLength(0); x++)
            {
                try
                {
                    // get pixel value
                    Color color = layoutAsColors[x, y];
                    // draw only walkable tiles
                    if (color != walkableColor)
                    {
                        continue;
                    }
                    // check if in dict
                    TileEntry tileEntry = roomManager.GetTile(color);
                    // get position of spawn
                    Vector3 nextPosition = getNextTilePosition(x, y, tileEntry.elevation);
                    // if (x == 1 || y == 1 || x == layoutAsColors.GetLength(1) - 2 || y == layoutAsColors.GetLength(0) - 2)
                    // {
                    // }
                    InstantiateTileAt(tileEntry, nextPosition, x, y);

                    // set center position
                    if (x == layoutAsColors.GetLength(1) / 2 && y == layoutAsColors.GetLength(0) / 2)
                    {
                        centerPosition = getNextTilePosition(x, y, 0);
                        // Debug.Log("Center position is " + centerPosition);
                    }
                }
                catch (IndexOutOfRangeException e)
                {
                    Debug.Log("(x,y) => (" + x + "," + y + ")");
                    Debug.Log(e.Message);
                }
            }

        }

        void InstantiateTileAt(TileEntry tileEntry, Vector3 position, int x, int y)
        {
            if (tileEntry.prefabs != null && tileEntry.prefabs.Length > 0)
            {
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
                    Debug.Log("Tile was null at cell position (" + position + ") ->");
                    Debug.Log(tileEntry);
                }
                // place tile in respective position
                Instantiate(tile, position, Quaternion.identity, roomsParent);
            }
            else
            {
                Debug.Log("Empty prefabs for tile " + tileEntry.color + " at position (" + x + "," + y + ")");
            }
        }

        roomsParent.GetComponent<CompositeColliderHandler>().SetInfo(gridPosition, centerPosition);
    }

}

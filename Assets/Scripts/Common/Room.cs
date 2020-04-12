using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
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
    public GameObject triggerPrefab;
    public Vector2 centerPosition { get; private set; }
    // in case this room was generated linked to other room
    // then will not spawn a collider in this direction 
    public char ignoreTriggerDirection = '+';
    private bool hasRequestedNeighbours = false;
    private RoomManager roomManager;
    private Vector3 startPosition;
    private Vector3Int roomDimensions;
    [SerializeField]
    // bright yellow
    private Color walkableColor = new Color(1, 1, 0, 1);
    private Transform roomsParent;

    private void Awake()
    {
        roomManager = GameObject.FindGameObjectWithTag("RoomManager").GetComponent<RoomManager>();
    }

    private void Start()
    {
        roomsParent = new GameObject("RoomParent").transform;
        roomsParent.SetParent(transform);

        // delay of 0.1 sec
        // so that we can see the map building
        // also roommanager start  function should get over by then
        Invoke("BuildRoom", 0.05f);
    }

    private void ReadLayout()
    {
        layoutAsColors = new Color[roomDimensions.x, roomDimensions.y];
        Color[] colors = roomLayout.GetPixels(0, 0, roomDimensions.x, roomDimensions.y);

        for (int index = 0, row = 0, col = 0; index < colors.Length; index++)
        {

            if (index != 0 && index % roomDimensions.x == 0)
            {
                // start new row
                row++;
            }
            col = index % roomDimensions.y;
            layoutAsColors[row, col] = colors[index];
        }
    }

    private void BuildRoom()
    {
        startPosition = transform.position;
        Name = transform.name.Split('(')[0];
        bool isValid = roomManager.RegisterRoom(Name, transform);
        if (!isValid)
        {
            Debug.Log("Room already exists at this position hence destroying. Position => " + startPosition);
            Destroy(gameObject);
        }
        roomDimensions = new Vector3Int(roomLayout.width, roomLayout.height, 0);
        ReadLayout();
        InstantiateTiles();
        CreateTriggers();
    }

    void CreateTriggers()
    {
        // 2nd pixel row and 2nd last pixel row
        // first run consists of hori as R and vert as T
        // second run consists of hori as L and vert as B

        int[] fixedAxis = new int[] { 1, layoutAsColors.GetLength(0) - 2 };
        for (int i = 0; i < fixedAxis.Length; i++)
        {
            // R - L
            var horiExitPoints = GetHoriPoints(i, fixedAxis[i]);
            // B - T
            var vertExitPoints = GetVertPoints(i, fixedAxis[i]);

            if (horiExitPoints.Count > 1)
            {
                // ignore B and T here
                Vector2 firstTriggerPosition = horiExitPoints[0];
                // spawn horizontal trigger
                var triggerObj = Instantiate(triggerPrefab, firstTriggerPosition, Quaternion.identity, transform);
                triggerObj.GetComponent<ExitTrigger>().Initialize(horiExitPoints);
            }
            if (vertExitPoints.Count > 1)
            {
                // ignore R and L here
                Vector2 firstTriggerPosition = vertExitPoints[0];
                // spawn vertical trigger
                var triggerObj = Instantiate(triggerPrefab, firstTriggerPosition, Quaternion.identity, transform);
                triggerObj.GetComponent<ExitTrigger>().Initialize(vertExitPoints);
            }
        }


        List<Vector2> GetHoriPoints(int key, int value)
        {
            var horiExitPoints = new List<Vector2>();

            // // ignore directions accordingly 
            // if (key == 1 && ignoreTriggerDirection == 'R')
            // {
            //     Debug.Log("Ignore R Collider");
            //     return horiExitPoints;
            // }
            // else if (key == 0 && ignoreTriggerDirection == 'L')
            // {
            //     Debug.Log("Ignore L Collider");
            //     return horiExitPoints;
            // }

            // if a walkable pixel is along the edge
            // add it into collider position so that 
            // later we can add a trigger collider along it
            for (int x = 0; x < layoutAsColors.GetLength(0); x++)
            {
                // check horizontal exits
                // fixedAxis == 2nd  -> R
                // fixedAxis == 2nd last -> L

                if (layoutAsColors[x, value] == walkableColor)
                {
                    horiExitPoints.Add(getNextTilePosition(x, value, 0));
                }
            }
            return horiExitPoints;
        }

        List<Vector2> GetVertPoints(int key, int value)
        {
            var vertExitPoints = new List<Vector2>();

            // // ignore directions accordingly 
            // if (key == 1 && ignoreTriggerDirection == 'T')
            // {
            //     Debug.Log("Ignore T Collider");
            //     return vertExitPoints;
            // }
            // else if (key == 0 && ignoreTriggerDirection == 'B')
            // {
            //     Debug.Log("Ignore B Collider");
            //     return vertExitPoints;
            // }

            // if a walkable pixel is along the edge
            // add it into collider position so that 
            // later we can add a trigger collider along it
            for (int x = 0; x < layoutAsColors.GetLength(0); x++)
            {
                // check horizontal exits
                // fixedAxis == 2nd  -> R
                // fixedAxis == 2nd last -> L

                if (layoutAsColors[value, x] == walkableColor)
                {
                    vertExitPoints.Add(getNextTilePosition(value, x, 0));
                }
            }
            return vertExitPoints;
        }
    }

    Vector2 getNextTilePosition(int w, int h, float elevation)
    {
        Vector2 rowStart = (Vector2)startPosition + tileColumnOffset * h;
        Vector2 rowTile = rowStart + w * tileRowOffset;
        rowTile.y += elevation * elevationOffset;
        return rowTile;
    }

    private void InstantiateTiles()
    {
        // for each pixel in map
        for (int y = 0; y < layoutAsColors.GetLength(0); y++)
        {
            for (int x = 0; x < layoutAsColors.GetLength(1); x++)
            {
                Color color = layoutAsColors[x, y];
                TileEntry tileEntry = roomManager.GetTile(color);
                Vector3 nextPosition = getNextTilePosition(x, y, tileEntry.elevation);
                // get position of spawn
                // check if in dict
                // get pixel value
                // if (x == 1 || y == 1 || x == layoutAsColors.GetLength(1) - 2 || y == layoutAsColors.GetLength(0) - 2)
                // {
                InstantiateTileAt(tileEntry, nextPosition);
                // }

                // set center position
                if (x == layoutAsColors.GetLength(1) / 2 && y == layoutAsColors.GetLength(0) / 2)
                {
                    centerPosition = getNextTilePosition(x, y, 0);
                    // Debug.Log("Center position is " + centerPosition);
                }
            }

        }

        void InstantiateTileAt(TileEntry tileEntry, Vector3 position)
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
                Debug.Log("Empty prefabs for tile " + tileEntry.color);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Room")
        {
            var isValid = roomManager.RegisterConflict(transform);
            if (!isValid)
            {
                Debug.LogError("Two rooms were spawned over each other. Hence destroying -> " + other.gameObject);
                Destroy(other.gameObject);
            }
        }
    }
}

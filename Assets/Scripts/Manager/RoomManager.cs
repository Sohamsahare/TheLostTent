using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    // Room manager will spawn all rooms according to doors
    // Rooms will initialise themselves according to its properties
    // keeps Single responsiblity rule intact
    public TileEntry[] tiles;
    private List<string> roomNames;
    private Dictionary<string, GameObject> nameToPrefabDictionary;
    [SerializeField]
    private GameObject[] roomPrefabs;
    public Texture2D layoutTexture;
    public int roomDimension = 16;
    public int roomsPerRowInLayout = 4;
    public int roomsPerColInLayout = 4;
    public float roomWidth = 13.83f;
    public float roomHeight = 8;
    public int maxRooms = 14;
    public int maxSpawnableRooms = 5;
    // private int currentRoomPosition = 0;

    [SerializeField]
    private string startRoom = "BTRL";
    [SerializeField]
    private Vector2 startPosition = Vector2.zero;
    // public 
    private Dictionary<Color, TileEntry> colorToTileDictionary;
    // private Dictionary<string, Color[,]> nameToLayoutDictionary;
    private List<Vector2> spawnedPositions;

    private Dictionary<char, List<string>> directionToRoomNameDictionary;
    private LevelManager levelManager;

    private void Awake()
    {
        levelManager = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LevelManager>();
    }

    private void Start()
    {
        spawnedPositions = new List<Vector2>();
        SpawnRooms();
    }

    // public void 

    public void SpawnRooms()
    {
        Initialize();
    }

    public void RequestNeighbourRoom(Vector2 roomCenterPosition, char spawnDirection)
    {
        if (!spawnedPositions.Contains(roomCenterPosition))
        {
            spawnedPositions.Add(roomCenterPosition);
        }


        // determine the type to spawn in the doordirection 
        char roomDirectionType = getOppositeDirection(spawnDirection);

        // build rooms in directions available
        Vector2 nextPosition = GetNextRoomPosition(roomCenterPosition, spawnDirection);

        Debug.Log("-----------------------------------------");
        Debug.Log("Spawning a " + roomDirectionType + " type in direction " + spawnDirection + " at " + nextPosition);
        if (spawnedPositions.Contains(nextPosition))
        {
            Debug.Log("Spawned at this posi already. Hence skipping...");
            return;
            // skip  spawning a room at that location
        }
        Debug.Log("-----------------------------------------");
        List<string> possibleRooms = directionToRoomNameDictionary[roomDirectionType];
        string nextRoomName = possibleRooms[UnityEngine.Random.Range(0, possibleRooms.Count)];
        GameObject roomObj = Instantiate(nameToPrefabDictionary[nextRoomName], nextPosition, Quaternion.identity, transform);
        roomObj.GetComponent<Room>().ignoreTriggerDirection = roomDirectionType;

        // spawn enemies when requested
        levelManager.SpawnAt(nextPosition + new Vector2(roomWidth, 0));
        Debug.DrawRay(nextPosition + new Vector2(roomWidth, 0), Vector2.up, Color.red, 5f);
    }

    public Vector2 GetNextRoomPosition(Vector2 roomCenterPosition, char direction)
    {
        Vector2 nextPosition = roomCenterPosition + new Vector2(roomWidth, roomHeight);
        switch (direction)
        {
            case 'L':
                nextPosition.x *= 1;
                nextPosition.y *= 1;
                return nextPosition;
            case 'R':
                nextPosition.x *= -1;
                nextPosition.y *= -1;
                return nextPosition;
            case 'T':
                nextPosition.x *= 1;
                nextPosition.y *= -1;
                return nextPosition;
            case 'B':
                nextPosition.x *= -1;
                nextPosition.y *= 1;
                return nextPosition;
            default:
                return nextPosition;
        }
    }

    public char getOppositeDirection(char direction)
    {
        switch (direction)
        {
            case 'T':
                return 'B';
            case 'B':
                return 'T';
            case 'R':
                return 'L';
            case 'L':
                return 'R';
            default:
                return '+';
        }
    }

    private void Initialize()
    {
        // store tile info
        colorToTileDictionary = new Dictionary<Color, TileEntry>();
        foreach (TileEntry tile in tiles)
        {
            colorToTileDictionary.Add(tile.color, tile);
            // Debug.Log("Adding " + tile.color + " to dictionary");
        }

        // populate room names
        roomNames = new List<string>();
        nameToPrefabDictionary = new Dictionary<string, GameObject>();
        foreach (GameObject prefab in roomPrefabs)
        {
            roomNames.Add(prefab.name);
            nameToPrefabDictionary.Add(prefab.name, prefab);
        }


        // get room directions
        int currentRoomNumber = 0;
        while (currentRoomNumber < maxRooms)
        {
            string roomName = roomNames[currentRoomNumber];
            RegisterRoomAsPerDirection(roomName);
            currentRoomNumber++;
        }
    }

    private void RegisterRoomAsPerDirection(string roomName)
    {
        // we will not classify starting room
        // as we do not want any more 4-way rooms to spawn in the dungeon
        if (roomName == startRoom)
        {
            return;
        }

        if (directionToRoomNameDictionary == null)
        {
            directionToRoomNameDictionary = new Dictionary<char, List<string>>();
        }

        foreach (char c in roomName)
        {
            switch (c)
            {
                case 'T':
                    if (directionToRoomNameDictionary.ContainsKey('T'))
                    {
                        directionToRoomNameDictionary['T'].Add(roomName);
                    }
                    else
                    {
                        directionToRoomNameDictionary.Add('T', new List<string>() { roomName });
                    }
                    break;
                case 'B':
                    if (directionToRoomNameDictionary.ContainsKey('B'))
                    {
                        directionToRoomNameDictionary['B'].Add(roomName);
                    }
                    else
                    {
                        directionToRoomNameDictionary.Add('B', new List<string>() { roomName });
                    }
                    break;
                case 'R':
                    if (directionToRoomNameDictionary.ContainsKey('R'))
                    {
                        directionToRoomNameDictionary['R'].Add(roomName);
                    }
                    else
                    {
                        directionToRoomNameDictionary.Add('R', new List<string>() { roomName });
                    }
                    break;
                case 'L':
                    if (directionToRoomNameDictionary.ContainsKey('L'))
                    {
                        directionToRoomNameDictionary['L'].Add(roomName);
                    }
                    else
                    {
                        directionToRoomNameDictionary.Add('L', new List<string>() { roomName });
                    }
                    break;
                default:
                    break;

            }
        }
    }

    public TileEntry GetTile(Color color)
    {
        if (colorToTileDictionary.ContainsKey(color))
        {
            // retrieve tile and return
            TileEntry tile = colorToTileDictionary[color];
            return tile;
        }
        else
        {
            return new TileEntry();
        }
    }
}

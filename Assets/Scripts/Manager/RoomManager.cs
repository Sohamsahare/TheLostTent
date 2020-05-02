using System;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using TheLostTent;

public class RoomManager : MonoBehaviour
{
    // Room manager will spawn all rooms according to doors
    // Rooms will initialise themselves according to its properties
    // keeps Single responsiblity rule intact
    public TileEntry[] tiles;
    public Texture2D layoutTexture;
    public int roomDimension = 16;
    public int roomsPerRowInLayout = 4;
    public int roomsPerColInLayout = 4;
    public float roomWidth = 13.83f;
    public float roomHeight = 8;
    public int maxSpawnableRooms = 5;
    private int spawnedRooms = 0;
    public Vector2Int gridSize = new Vector2Int(4, 4);

    [SerializeField]
    private GameObject[] roomPrefabs;
    [SerializeField]
    private string startRoom = "BTRL";
    [SerializeField]
    private Vector2 startPosition = Vector2.zero;
    private List<string> roomNames;
    private Dictionary<string, GameObject> nameToPrefabDictionary;
    // public 
    private Dictionary<Color, TileEntry> colorToTileDictionary;
    // private Dictionary<string, Color[,]> nameToLayoutDictionary;
    private HashSet<RoomInfo> roomInfos;
    private Dictionary<char, List<string>> directionToRoomNameDictionary;
    private RoomInfo[,] rooms;
    private LevelManager levelManager;
    private HashSet<Vector2> conflictPositions;
    private Vector2Int currentGridPosition;
    [SerializeField]
    private int virtualCameraPriority = 99999;
    private Transform playerTransform;
    private Vector2Int lastRoom;
    private float[] terminalProbabilities;
    [SerializeField]
    private float incrementalProbability = .33f;

    private void Awake()
    {
        levelManager = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LevelManager>();
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Start()
    {
        terminalProbabilities = new float[4];
        roomInfos = new HashSet<RoomInfo>();
        conflictPositions = new HashSet<Vector2>();
        playerTransform.GetComponentInChildren<Heart>().deathEvent += () =>
        {
            virtualCameraPriority += 2;
            rooms[gridSize.x, gridSize.y].camera.Priority = virtualCameraPriority;
        };
        CreateDungeon();
        Debug.Log("We have " + roomPrefabs.Length + " room prefabs");
    }

    // Initialises all variables and spawns rooms in a recursive manner 
    public void CreateDungeon()
    {
        Initialize();
        SpawnRooms();
    }

    // replace/correct this method
    private void SpawnRooms()
    {
        Vector2Int gridPosition = gridSize;
        currentGridPosition = gridPosition;
        // initialise starting room
        SpawnRoomRecursive(startRoom, gridPosition, startPosition, true);
    }
    // replace/correct this method
    void SpawnRoomRecursive(string roomName, Vector2Int gridPosition, Vector2 roomPosition, bool isFirstRoom = false, int pathIndex = -1)
    {
        // stop spawning rooms if terminal probability reached
        if (pathIndex != -1 && terminalProbabilities[pathIndex] > 1)
        {
            return;
        }

        // instantiate room
        GameObject roomObj = Instantiate(nameToPrefabDictionary[roomName], roomPosition, Quaternion.identity, transform);
        roomObj.GetComponent<RoomBuilder>().SetGridPosition(gridPosition);

        // intialize virtual camera 
        CinemachineVirtualCamera camera = roomObj.GetComponentInChildren<CinemachineVirtualCamera>();
        if (isFirstRoom)
        {
            // enable first room camera first
            camera.Priority = virtualCameraPriority + 1;
            lastRoom = gridPosition;
        }
        else
        {
            // lesser priority will ignore the camera's view
            camera.Priority = virtualCameraPriority;
        }
        // follow player transform
        // camera.Follow = playerTransform;

        rooms[gridPosition.x, gridPosition.y] = new RoomInfo(roomName, roomPosition, roomObj, camera);

        spawnedRooms++;

        foreach (char direction in roomName)
        {
            char oppositeDirection = getOppositeDirection(direction);
            Vector2Int nextGridPosition = getNextGridPosition(oppositeDirection, gridPosition);
            if (nextGridPosition.x < rooms.GetLength(0) && nextGridPosition.y < rooms.GetLength(1))
            {
                if (rooms[nextGridPosition.x, nextGridPosition.y].roomObj == null)
                {
                    // this is an empty room so spawn a new one

                    // calculate necessary values 
                    Vector2 nextPosition = GetNextRoomPosition(roomPosition, direction);

                    // List<string> possibleRooms = directionToRoomNameDictionary[oppositeDirection];
                    // string nextRoomName = possibleRooms[UnityEngine.Random.Range(0, possibleRooms.Count)];
                    string nextRoomName = GetNextPossibleRoom(oppositeDirection, pathIndex);

                    if (spawnedRooms < maxSpawnableRooms)
                    {
                        Debug.Log($"Spawning {nextRoomName} of type {oppositeDirection} at grid {nextGridPosition} to join in direction {direction} for gridPos {gridPosition}");
                        int localPathIndex = pathIndex;
                        if (localPathIndex == -1)
                        {
                            switch (direction)
                            {
                                case 'T':
                                    localPathIndex = 0;
                                    break;
                                case 'B':
                                    localPathIndex = 1;
                                    break;
                                case 'L':
                                    localPathIndex = 2;
                                    break;
                                case 'R':
                                    localPathIndex = 3;
                                    break;
                                default:
                                    break;
                            }
                        }
                        SpawnRoomRecursive(nextRoomName, nextGridPosition, nextPosition, false, localPathIndex);
                    }
                }
                else
                {
                    // this room is already occupied
                    Debug.Log(nextGridPosition + " is already occupied");
                }
            }
            else
            {
                Debug.Log("Invalid grid position to place a room " + nextGridPosition);
            }
        }
    }

    private string GetNextPossibleRoom(char oppositeDirection, int pathIndex)
    {
        List<string> possibleRooms = directionToRoomNameDictionary[oppositeDirection];
        string nextRoomName = possibleRooms[UnityEngine.Random.Range(0, possibleRooms.Count)];
        if (pathIndex > -1 && pathIndex < terminalProbabilities.Length)
        {
            terminalProbabilities[pathIndex] += incrementalProbability;
        }
        if (pathIndex > -1 && terminalProbabilities[pathIndex] >= .9f)
        {
            nextRoomName = oppositeDirection + "";
        }
        return nextRoomName;
    }

    public void SetCurrentRoom(Vector2Int gridPos)
    {
        // set camera priority to display that room
        if (gridPos.x < rooms.GetLength(0) && gridPos.y < rooms.GetLength(1))
        {
            RoomInfo roomInfo = rooms[gridPos.x, gridPos.y];
            CinemachineVirtualCamera camera = roomInfo.camera;
            // increase priority ot override last camera which is displaying
            if (!lastRoom.Equals(gridPos))
            {
                virtualCameraPriority += 2;
                camera.Priority = virtualCameraPriority;
                Debug.Log($"Increasing priority {virtualCameraPriority} and changing current room at {gridPos}");
            }
            lastRoom = gridPos;
        }

    }

    public Vector2 GetNextRoomPosition(Vector2 roomCenterPosition, char direction)
    {
        Vector2 nextPosition = new Vector2(roomWidth, roomHeight);
        switch (direction)
        {
            case 'L':
                nextPosition *= new Vector2Int(-1, 1);
                break;
            case 'R':
                nextPosition *= new Vector2Int(1, -1);
                break;
            case 'T':
                nextPosition *= new Vector2Int(1, 1);
                break;
            case 'B':
                nextPosition *= new Vector2Int(-1, -1);
                break;
            default:
                Debug.Log("Next room position invalid! -> " + direction);
                break;
        }
        nextPosition += roomCenterPosition;
        return nextPosition;
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
                Debug.Log("Opposite direction invalid! -> " + direction);
                return '+';
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

    private Vector2Int getNextGridPosition(char direction, Vector2Int gridPosition)
    {
        Vector2Int nextGridPosition = gridPosition;

        switch (direction)
        {
            case 'B':
                nextGridPosition += Vector2Int.down;
                break;
            case 'T':
                nextGridPosition += Vector2Int.up;
                break;
            case 'R':
                nextGridPosition += Vector2Int.right;
                break;
            case 'L':
                nextGridPosition += Vector2Int.left;
                break;
            default:
                Debug.Log("Invalid Direction for next grid position");
                break;
        }
        return nextGridPosition;
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
        while (currentRoomNumber < roomNames.Count)
        {
            string roomName = roomNames[currentRoomNumber];
            RegisterRoomAsPerDirection(roomName);
            currentRoomNumber++;
        }

        rooms = new RoomInfo[gridSize.x * 2, gridSize.y * 2];
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
                    addToRoomDictionary(roomName, 'T');
                    break;
                case 'B':
                    addToRoomDictionary(roomName, 'B');
                    break;
                case 'R':
                    addToRoomDictionary(roomName, 'R');
                    break;
                case 'L':
                    addToRoomDictionary(roomName, 'L');
                    break;
                default:
                    break;

            }
        }

        void addToRoomDictionary(string _roomName, char _direction)
        {
            if (directionToRoomNameDictionary.ContainsKey(_direction))
            {
                directionToRoomNameDictionary[_direction].Add(_roomName);
            }
            else
            {
                directionToRoomNameDictionary.Add(_direction, new List<string>() { _roomName });
            }
        }
    }
}

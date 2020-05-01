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

    private void Awake()
    {
        levelManager = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LevelManager>();
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Start()
    {
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

    public void setCurrentRoom(Vector2Int gridPos)
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
                nextPosition.x *= 1;
                nextPosition.y *= 1;
                nextPosition += roomCenterPosition;
                return nextPosition;
            case 'R':
                nextPosition.x *= -1;
                nextPosition.y *= -1;
                nextPosition += roomCenterPosition;
                return nextPosition;
            case 'T':
                nextPosition.x *= 1;
                nextPosition.y *= -1;
                nextPosition += roomCenterPosition;
                return nextPosition;
            case 'B':
                nextPosition.x *= -1;
                nextPosition.y *= 1;
                nextPosition += roomCenterPosition;
                return nextPosition;
            default:
                Debug.Log("Next room position invalid! -> " + direction);
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
                Debug.Log("Opposite direction invalid! -> " + direction);
                return '+';
        }
    }

    public void EnableRoomAt(ExitOrientation orientation)
    {
        Debug.Log("Enabling room in direction " + orientation);
        Vector2Int movement = Vector2Int.zero;
        switch (orientation)
        {
            case ExitOrientation.T:
                movement = Vector2Int.up;
                break;
            case ExitOrientation.B:
                movement = Vector2Int.down;
                break;
            case ExitOrientation.L:
                movement = Vector2Int.left;
                break;
            case ExitOrientation.R:
                movement = Vector2Int.right;
                break;
            default:
                Debug.Log("Invalid Orientation");
                break;
        }
        currentGridPosition += movement;
        var room = rooms[currentGridPosition.x, currentGridPosition.y];
        if (room.roomObj != null)
        {
            Debug.Log("Retrieving room => " + room.ToString() + " at " + currentGridPosition);
            room.roomObj.SetActive(true);
        }
        else
        {
            Debug.Log("Null room " + currentGridPosition);
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

    // Initialises all variables and spawns rooms in a recursive manner 
    public void CreateDungeon()
    {
        Initialize();
        SpawnRooms();
    }

    private void SpawnRooms()
    {
        Vector2Int gridPosition = gridSize;
        currentGridPosition = gridPosition;
        // initialise starting room
        SpawnRoomRecursive(startRoom, gridPosition, startPosition, true);
    }
    void SpawnRoomRecursive(string roomName, Vector2Int gridPosition, Vector2 roomPosition, bool isFirstRoom = false)
    {
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

                    List<string> possibleRooms = directionToRoomNameDictionary[oppositeDirection];
                    string nextRoomName = possibleRooms[UnityEngine.Random.Range(0, possibleRooms.Count)];

                    if (spawnedRooms < maxSpawnableRooms)
                    {
                        // Debug.Log($"Spawning {nextRoomName} of type {oppositeDirection} at grid {nextGridPosition} to join in direction {direction} for gridPos {gridPosition}");
                        SpawnRoomRecursive(nextRoomName, nextGridPosition, nextPosition);
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
            case 'L':
                nextGridPosition += Vector2Int.right;
                break;
            case 'R':
                nextGridPosition += Vector2Int.left;
                break;
            default:
                Debug.Log("Invalid Direction for next grid position");
                break;
        }
        return nextGridPosition;
    }

    private void PlaceRoom(Vector2 roomCenterPosition, char spawnDirection)
    {

        // determine the type to spawn in the doordirection 
        char roomDirectionType = getOppositeDirection(spawnDirection);

        // build rooms in directions available
        Vector2 nextPosition = GetNextRoomPosition(roomCenterPosition, spawnDirection);

        List<string> possibleRooms = directionToRoomNameDictionary[roomDirectionType];
        string nextRoomName = possibleRooms[UnityEngine.Random.Range(0, possibleRooms.Count)];
        // if (roomInfos.Contains(new RoomInfo(nextRoomName, nextPosition)))
        // {
        //     Debug.Log("Ignoring placing room as it already exists. " + nextPosition);
        //     return;
        // }

        // Debug.Log("Spawning a " + roomDirectionType + " type in direction " + spawnDirection + " at " + nextPosition + " i.e. room -> " + nextRoomName);
        GameObject roomObj = Instantiate(nameToPrefabDictionary[nextRoomName], nextPosition, Quaternion.identity, transform);
        // roomObj.GetComponent<Room>().ignoreTriggerDirection = roomDirectionType;
        // spawn enemies when requested
        // levelManager.SpawnAt(nextPosition + new Vector2(roomWidth, 0));
        // Debug.DrawRay(nextPosition + new Vector2(roomWidth, 0), Vector2.up, Color.red, 5f);
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
}

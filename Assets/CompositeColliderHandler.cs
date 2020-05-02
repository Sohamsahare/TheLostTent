using System.Collections;
using System.Collections.Generic;
using TheLostTent;
using UnityEngine;

public class CompositeColliderHandler : MonoBehaviour
{

    public Vector2Int gridPosition { private set; get; }
    public Vector2 centerPosition { private set; get; }
    private LevelManager levelManager;
    private RoomManager roomManager;
    private Transform playerTransform;
    private bool enemiesSpawned;

    private void Awake()
    {
        levelManager = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LevelManager>();
        roomManager = GameObject.FindGameObjectWithTag("RoomManager").GetComponent<RoomManager>();
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Start()
    {
        enemiesSpawned = false;
        playerTransform.GetComponentInChildren<Heart>().deathEvent += () =>
        {
            enemiesSpawned = false;
        };
    }

    public void SetInfo(Vector2Int gridPosition, Vector2 centerPosition)
    {
        this.gridPosition = gridPosition;
        this.centerPosition = centerPosition;
    }

    // private void OnCollisionEnter2D(Collision2D other)
    // {
    //     Debug.Log(other.transform.name + " just entered " + transform.parent.name);
    //     if (other.transform.tag == "Player")
    //     {
    //         if (!enemiesSpawned)
    //         {
    //             levelManager.SpawnAt(centerPosition);
    //             enemiesSpawned = !enemiesSpawned;
    //         }
    //         else
    //         {
    //             Debug.LogWarning("Enemies already spawned at " + centerPosition);
    //         }
    //     }
    // }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.transform.tag == "Player")
        {
            // Debug.Log(other.transform.name + " just entered " + transform.parent.name);
            if (!enemiesSpawned)
            {
                levelManager.SpawnAt(centerPosition);
                enemiesSpawned = !enemiesSpawned;
            }
            else
            {
                // Debug.LogWarning("Enemies already spawned at " + centerPosition);
            }
            roomManager.SetCurrentRoom(gridPosition);
        }
    }
}

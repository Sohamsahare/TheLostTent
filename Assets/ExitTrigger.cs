using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitTrigger : MonoBehaviour
{
    // public string Direction 
    private Room parentRoom;
    private List<Vector2> triggerPoints;
    public ExitOrientation orientation { get; private set; }
    private void Awake()
    {
        parentRoom = GetComponentInParent<Room>();
    }
    public void Initialize(List<Vector2> triggerPoints)
    {
        if (triggerPoints.Count == 1)
        {
            Debug.Log("Invalid trigger points. Destroying self");
            Destroy(gameObject);
        }
        // Debug.Log("COmmencing trigger initialization for # points" + triggerPoints.Count);
        this.triggerPoints = triggerPoints;
        // check if end points lie on the same x coordinate
        bool isHorizontal = !!(triggerPoints[0].x == triggerPoints[triggerPoints.Count - 1].x);
        // check direction relative to room center
        Vector2 roomPosition = parentRoom.centerPosition;
        Vector2 relativeDirection = triggerPoints[triggerPoints.Count / 2] - roomPosition;
        relativeDirection = relativeDirection.normalized;
        float angle = Vector2.SignedAngle(Vector2.right, relativeDirection);
        if (-90 <= angle && angle <= 0)
        {
            orientation = ExitOrientation.T;
        }
        else if (0 <= angle && angle <= 90)
        {
            orientation = ExitOrientation.L;
        }
        else if (90 <= angle && angle <= 180)
        {
            orientation = ExitOrientation.B;
        }
        // if angle between -180 and -90
        else
        {
            orientation = ExitOrientation.R;
        }

        triggerPoints = MakeRelativeToFirstPoint(triggerPoints);
        // add edge collider 2d object 
        var trigger = gameObject.AddComponent<EdgeCollider2D>();
        trigger.isTrigger = true;
        trigger.points = triggerPoints.ToArray();
    }

    private List<Vector2> MakeRelativeToFirstPoint(List<Vector2> triggerPoints)
    {
        var list = new List<Vector2>();
        var first = triggerPoints[0];
        for (int i = 0; i < triggerPoints.Count; i++)
        {
            var relative = triggerPoints[i] - first;
            list.Add(relative);
        }
        return list;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            // enable next room 
            Char orientationChar = ' ';
            Char.TryParse(orientation.ToString(), out orientationChar);
            Debug.LogWarning("Passed trigger on " + transform.parent.name);

            var roomManager = GameObject
                .FindGameObjectWithTag("RoomManager")
                .GetComponent<RoomManager>();

            GameObject
                .FindGameObjectWithTag("LevelManager")
                .GetComponent<LevelManager>()
                .RequestEnemiesAtRoom(parentRoom.transform.position, orientationChar, roomManager.roomWidth, roomManager.roomHeight);
            // Destroy(gameObject);
        }
    }
}

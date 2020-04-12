using System;
using UnityEngine;

public struct RoomInfo : IEquatable<RoomInfo>
{
    public string Name;
    // public Transform transform;
    // public Vector2 spawnPosition { get { return transform.position; } }
    public Vector2 spawnPosition;

    public RoomInfo(string Name, Vector2 position)
    {
        this.Name = Name;
        // this.transform = transform;
        this.spawnPosition = position;
    }

    public override string ToString()
    {
        return "Room " + Name + " at position " + spawnPosition;
    }

    public override bool Equals(object obj)
    {
        RoomInfo objAsRoomInfo = (RoomInfo)obj;
        return Equals(objAsRoomInfo);
    }
    public override int GetHashCode()
    {
        return spawnPosition.GetHashCode();
    }
    public bool Equals(RoomInfo other)
    {
        var otherPos = other.spawnPosition;
        if (spawnPosition.x == otherPos.x && spawnPosition.y == otherPos.y)
        {
            Debug.Log("Current -> " + spawnPosition + " Is equal to other -> " + other.spawnPosition);
            return true;
        }
        else
        {
            return false;
        }
    }
}
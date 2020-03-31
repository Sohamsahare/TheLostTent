using UnityEngine;
[System.Serializable]
public struct ObjectPool
{
    public string tag;
    public Transform parent;
    public GameObject prefab;
    public int capacity;
    public ObjectPool(string tag, int capacity, Transform parent, GameObject prefab)
    {
        this.tag = tag;
        this.capacity = capacity;
        this.parent = parent;
        this.prefab = prefab;
    }
}

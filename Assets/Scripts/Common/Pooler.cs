using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;

public class Pooler : MonoBehaviour
{
    private Dictionary<string, Queue<GameObject>> tagToPoolDictionary;
    public ObjectPool[] objectPools;

    private void Start()
    {
        InitialiseAllPools();
    }

    public GameObject GetObject(string tag, Vector3 position, Vector3 rotation, Transform parent = null)
    {
        if (tagToPoolDictionary.ContainsKey(tag))
        {
            GameObject poolObj;
            poolObj = tagToPoolDictionary[tag].Dequeue();
            poolObj.transform.position = position;
            poolObj.transform.eulerAngles = rotation;
            poolObj.transform.SetParent(parent);
            poolObj.SetActive(true);
            return poolObj;
        }
        else
        {
            // invalid tag
            return null;
        }
    }

    public void DisableObj(string tag, GameObject obj, float life = 0)
    {
        if (tagToPoolDictionary.ContainsKey(tag))
        {
            // handle life
            StartCoroutine(DisableAfterSeconds(tag, obj, life));
        }
        else
        {
            // invalid key 
            Debug.Log("Invalid key while pooling back " + tag);
        }
    }

    IEnumerator DisableAfterSeconds(string tag, GameObject obj, float life)
    {
        yield return new WaitForSeconds(life);
        obj.SetActive(false);
        obj.transform.SetParent(transform);
        tagToPoolDictionary[tag].Enqueue(obj);
    }

    private void InitialiseAllPools()
    {
        // initialise dictionary
        tagToPoolDictionary = new Dictionary<string, Queue<GameObject>>();

        // generate objects for each pool and add them in respective queues
        // add the queue and the tag in dictionary
        foreach (ObjectPool pool in objectPools)
        {
            Queue<GameObject> queue = new Queue<GameObject>();
            for (int i = 0; i < pool.capacity; i++)
            {
                GameObject clone = Instantiate(pool.prefab, transform.position, Quaternion.identity, pool.parent);
                clone.SetActive(false);
                queue.Enqueue(clone);
            }
            tagToPoolDictionary.Add(pool.tag, queue);
        }
    }


}
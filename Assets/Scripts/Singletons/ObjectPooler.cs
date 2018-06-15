using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ObjectPooler : Singleton<ObjectPooler>
{
    [SerializeField] private List<ObjectPoolItem> itemsToPool;
    private List<GameObject> pooledObjects;

    private void Start()
    {
        pooledObjects = new List<GameObject>();
        foreach (var item in itemsToPool)
        {
            for (int i = 0; i < item.amountToPool; i++)
            {
                GameObject obj = Instantiate(item.objectToPool);
                obj.SetActive(false);
                pooledObjects.Add(obj);
            }
        }
    }

    public GameObject GetPooledObject(string tag)
    {
        GameObject obj = pooledObjects.FirstOrDefault(pooledObject => !pooledObject.activeInHierarchy && pooledObject.tag == tag);
        if (obj != null)
            return obj;

        foreach (var item in itemsToPool)
        {
            if (item.objectToPool.tag == tag)
            {
                if (item.shouldExpand)
                {
                    obj = Instantiate(item.objectToPool);
                    obj.SetActive(false);
                    pooledObjects.Add(obj);
                    return obj;
                }
            }
        }
        return null;
    }
}

[System.Serializable]
public class ObjectPoolItem
{
    public GameObject objectToPool;
    public int amountToPool;
    public bool shouldExpand;
}

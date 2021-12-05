using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// This class handles performance issues while loading and unloading chunks.
/// Instead of destroying and initializing new chunks, unloaded chunk is set to
/// inactive and loaded to active, which means gameobjects are being reused.
/// </summary>
public class ChunkPool : MonoBehaviour, ObjectPoolInterface
{
    public static ChunkPool instance;
    private List<GameObject> pooledObjects = new List<GameObject>();
    public int amountToPool;
    
    [SerializeField] private GameObject chunkPrefab;

    public void Awake(){
        //singleton
        if (instance == null)
        {
            instance = this;
        }
    }

    /// <summary>
    /// Initialize inactive gameobjects in pool to begin with.
    /// </summary>
    public void Start()
    {
        for (int i = 0; i < amountToPool; i++)
        {
            GameObject chunkP = Instantiate(chunkPrefab, new Vector3(0,0,0), Quaternion.identity);
            chunkP.transform.parent = gameObject.transform;
            chunkP.SetActive(false);
            pooledObjects.Add(chunkP);
        }
    }
    /// <summary>
    /// Use this method instead of initialization when loading chunk. This will return
    /// inactive gameobject in pool.
    /// </summary>
    /// <returns>Inactive gameobject or null.</returns>
    public GameObject GetPooledObject(){
        for (int i = 0; i < pooledObjects.Count; i++)
        {
            if (!pooledObjects[i].activeInHierarchy)
            {
                return pooledObjects[i];
            }
        }
        return null;
    }
}
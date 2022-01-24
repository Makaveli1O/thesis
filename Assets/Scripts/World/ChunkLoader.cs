using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class ChunkLoader : MonoBehaviour
{
    ObjectPool chunkPool;
    public TDMap map;
    private Dictionary<int2, GameObject> renderedChunks = new Dictionary<int2, GameObject>();

    /// <summary>
    /// Loop through all chunks, and determine whenever chunk should be loaded or 
    /// unloaded by calculating distance and checking treshold.
    /// </summary>
    /// <param name="PlayerPos">Position of player in the world</param>
    /// <param name="distToLoad">Distance treshold to load chunk</param>
    /// <param name="distToUnload">Distance treshold to unload chunk</param>
    public void LoadChunks(Vector3 PlayerPos, float distToLoad, float distToUnload)
    {
        int offset = 16; //chunk 32 -> 16, 16 is center
        for (int x = 0; x < map.width; x += Const.CHUNK_SIZE)
        {
            for (int y = 0; y < map.height; y += Const.CHUNK_SIZE)
            {
                //calcualte distance to the middle of chunk
                int chunkX = x + offset;
                int chunkY = y + offset;
                float dist = Vector2.Distance(new Vector2(chunkX, chunkY), new Vector2(PlayerPos.x, PlayerPos.y));
                if (dist < distToLoad)
                {
                    if (!renderedChunks.ContainsKey(new int2(x, y)))
                    {
                        CreateChunk(x, y);
                    }
                }
                else if (dist > distToUnload)
                {
                    if (renderedChunks.ContainsKey(new int2(x, y)))
                    {
                        UnloadChunk(x, y);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Unloads unnecessary chunk from pool, and removes it from rendered chunks dictionary.
    /// </summary>
    /// <param name="x">coord x</param>
    /// <param name="y">coord y</param>
    private void UnloadChunk(int x, int y)
    {
        //reference to the script, attached to chunk about to remove
        ChunkCreator chunkCreator = renderedChunks[new int2(x, y)].GetComponent<ChunkCreator>();
        chunkCreator.UnloadTrees();
        chunkCreator.UnloadObjects();
        //deactivate chunk
        renderedChunks[new int2(x, y)].SetActive(false);
        renderedChunks.Remove(new int2(x, y));
    }

    /// <summary>
    /// Pools chunk from the pool.
    /// </summary>
    /// <param name="x">x coord</param>
    /// <param name="y">y coords</param>
    private void CreateChunk(int x, int y)
    {
        int2 chunkKey = new int2(x,y);
        //create chunk object
        GameObject chunkP = chunkPool.GetPooledObject();
        if (chunkP != null)
        {
            chunkP.transform.parent = gameObject.transform;
            chunkP.SetActive(true);
            ChunkCreator chunkCreator = chunkP.GetComponent<ChunkCreator>(); //reference to script

            //create mesh (chunk) and save it
            Mesh mesh = new Mesh();
            map.chunks[chunkKey].chunkMesh = chunkCreator.CreateTileMesh(map.chunks[chunkKey], mesh);
            renderedChunks.Add(chunkKey, chunkP);
            //OnChunkTreesRequest?.Invoke(this, new OnChunkTreesRequestArgs{ chunk = map.chunks[chunkKey], obj = chunkP });
            
            //cache metadata
            //SaveChunk save = new SaveChunk(new Vector3(x, y, 0));
            //gameHandler.Save<WorldChunk>(map.chunks[chunkKey], ObjType.Chunk, new Vector3(x, y, 0));
        }
    }

    private void Awake(){
        chunkPool = GetComponent<ObjectPool>();
    }
}
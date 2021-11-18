using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

// INHERITANCE PROBLEM, DOES NOT SHARE ATTRIBUTES ASSIGNED IN INSPECTOR
public class ChunkLoader :  Map
{
    //problem with inheriting serialized variable, that has been adjusted in inspector
    private int map_w;
    private int map_h;
    private Dictionary<int2, GameObject> pooledChunks; //object pooling storage
    public ChunkLoader(){
        Dictionary<int2, GameObject> pooledChunks = new Dictionary<int2,GameObject>(); //list for object pooling
    }
    public void Init(int w, int h){
        this.map_w = w;
        this.map_h = h;
    }
    
    /// <summary>
    /// Loop through all chunks, and determine whenever chunk should be loaded or 
    /// unloaded by calculating distance and checking treshold.
    /// </summary>
    /// <param name="PlayerPos">Position of player in the world</param>
    /// <param name="distToLoad">Distance treshold to load chunk</param>
    /// <param name="distToUnload">Distance treshold to unload chunk</param>
    public void LoadChunks(Vector3 PlayerPos, float distToLoad, float distToUnload){
		for(int x = 0; x < map_w; x+=chunkSize){
			for(int y = 0; y < map_h ; y+=chunkSize){
				float dist=Vector2.Distance(new Vector2(x,y),new Vector2(PlayerPos.x,PlayerPos.y));
                if(dist<distToLoad){
                    if(!renderedChunks.ContainsKey(new int2(x,y))){
						CreateChunk(x,y);
					}
				} else if(dist>distToUnload){
					if(renderedChunks.ContainsKey(new int2(x,y))){
						UnloadChunk(x,y);
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
    private void UnloadChunk(int x, int y){
		Object.Destroy(chunks[new int2(x, y)].chunkMesh);
        renderedChunks.Remove(new int2(x,y));
    }
    /// <summary>
    /// Pools chunk from the pool.
    /// </summary>
    /// <param name="x">x coord</param>
    /// <param name="y">y coords</param>
    private void CreateChunk(int x, int y){
        //create chunk object
        //GameObject chunkP = Instantiate(chunkPrefab, new Vector3(0,0,0), Quaternion.identity);
        GameObject chunkP = ObjectPool.instance.GetPooledChunk();
        chunkP.transform.parent = gameObject.transform;
        ChunkCreator chunkCreator = chunkP.GetComponent<ChunkCreator>(); //reference to script
        //create mesh (chunk) and save it to structure holding chunk
        chunks[new int2(x,y)].chunkMesh = chunkCreator.CreateTileMesh(chunkSize,chunkSize, x, y);
        renderedChunks.Add(new int2(x,y), chunkP);
    }
    
}


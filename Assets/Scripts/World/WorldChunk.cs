using UnityEngine;
using Unity.Mathematics; 

/// <summary>
/// Class representing single chunk in the map.
/// </summary>
public class WorldChunk
{
    public int2 position; //start corner of chunk
    public TDTile[,] sample; //2d tile map for chunk
    public int[,] treeMap; //2d tree map
    public Mesh chunkMesh;
    public WorldChunk(int chunkSize = 32){
        sample = new TDTile[chunkSize, chunkSize];
        treeMap = new int[chunkSize, chunkSize];
    }
}
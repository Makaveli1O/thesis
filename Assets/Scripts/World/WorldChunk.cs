using UnityEngine;
using Unity.Mathematics; 
using System.Runtime.Serialization;

/// <summary>
/// Class representing single chunk in the map.
/// </summary>
public class WorldChunk
{
    public int2 position; //start corner of chunk
    public TDTile[,] sample; //2d tile map for chunk
    public int[,] treeMap; //2d tree map
    public Mesh chunkMesh; //mesh object attached to chunk
    public BiomePreset chunkBiome; //biome that chunk is most covered in
    public WorldChunk(){
        sample = new TDTile[Const.CHUNK_SIZE, Const.CHUNK_SIZE];
        treeMap = new int[Const.CHUNK_SIZE, Const.CHUNK_SIZE];
    }
}
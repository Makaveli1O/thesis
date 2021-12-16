using UnityEngine;
using Unity.Mathematics; 
using System.Collections.Generic;
using System.Runtime.Serialization;

/// <summary>
/// Class representing single chunk in the map.
/// </summary>
[System.Serializable]
public class WorldChunk
{
    public int2 position; //start corner of chunk
    [System.NonSerialized] public TDTile[,] sample; //2d tile map for chunk
    [System.NonSerialized] public int[,] treeMap; //2d tree map
    [System.NonSerialized] public int[,] zIndexMap; //2d tree map
    public List<string> trees; //trees, rocks etc. within chunk names
    public List<ObjectsStorage> objects;
    [System.NonSerialized] public Mesh chunkMesh; //mesh object attached to chunk
    public WorldChunk(){
        sample = new TDTile[Const.CHUNK_SIZE, Const.CHUNK_SIZE];
        treeMap = new int[Const.CHUNK_SIZE, Const.CHUNK_SIZE];
        zIndexMap = new int[Const.CHUNK_SIZE, Const.CHUNK_SIZE];
        trees = new List<string>();
        objects = new List<ObjectsStorage>();
    }
    [System.Serializable]
    public class ObjectsStorage
    {
        public int2 pos;
        public string sprite;
    }
}
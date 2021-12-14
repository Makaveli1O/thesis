using System.Collections.Generic;
using Unity.Mathematics; //int2

/// <summary>
/// Structure holding map dimensions intel
/// </summary>
[System.Serializable]
public struct TDMap 
{
    private static readonly object padlock = new object();
    
    public Dictionary<int2, WorldChunk> chunks;
    public int width;
    public int height;
    public int renderDistance;

}

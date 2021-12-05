using Unity.Mathematics; //int2
/// <summary>
/// TDTile struct is holding intel about each tile.
/// This intel is later used to determine it's type,
/// position in the world etc.
/// </summary>
public class TDTile
{  
    private bool walkable;
    public bool IsWalkable
    {
        set{walkable = value;}
        get{return walkable;}
    }
    public int z_index;
    public int2 pos;
    public BiomePreset biome;
    public float temperature;
    public float height;
    public float precipitation;
    public bool landmass;
    public string waterType;
    /*
        This comes in handy for things, such as creating paths, bitmasking, or flood filling
    */
    public TDTile left;
    public TDTile topLeft;
    public TDTile top;
    public TDTile topRight;
    public TDTile right;
    public TDTile bottomRight;
    public TDTile bottom;
    public TDTile bottomLeft;
    //type of edge between biomes (for smooth transition)
    public EdgeType edgeType;
    //type of edge of hill
    public EdgeType hillEdge;
    //trees
    public float treeValue;

}
public enum EdgeType
{
    //regular
    none,
    left,   
    top,    
    right,  
    bot,
    //two sides
    botLeft,
    topLeft,
    botRight,
    topRight,
    //corners
    botLeftOnly,
    topLeftOnly,
    botRightOnly,
    topRightOnly,
    //rare cases
    rareTRB, //TRB -> top, right, bottom
    rareLTR,
    rareRBL,
    rareBLT,
    rareTB,
    //cliffs
    cliff,
    cliffLeft,
    cliffEndLeft,
    cliffRight,
    cliffEndRight,
    cliffBot,
    cliffEndBot
}

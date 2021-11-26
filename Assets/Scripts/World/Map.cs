using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Unity.Mathematics;
using Random = UnityEngine.Random;


/// <summary>
/// Map generating is composed by 3 individual maps that are being generated within ChunkGenerator. 
/// Each map has its own meaning. 
    
/// Height map indicates altitude of each tile within map. 
/// Heat map indicates warmth of the area(tiles in cluster).
/// Moisture indicates humidity within area.
/// Each map differs by a seed.
/// </summary>
public class Map : MonoBehaviour
{
    /*
        Object references
    */
    ChunkGenerator chunkGenerator = null;


    [SerializeField] protected GameObject player;

    //store all biomes and prefabs for each tile
    //public BiomePreset[] biomes;
    public List<BiomePreset> biomes = new List<BiomePreset>();
    public GameObject chunkPrefab;
    //Map dimensions
    [Header("Dimensions")]
    public bool chunkLoading; //checkbox
    public int width = 32;
    public int height = 32;
    public float scale = 1.0f; 
    public int seed;
    public int heightSeed;
    public int precipitationSeed;
    /*
        Loaded chunks
    */
    protected int chunkSize = 32;
    
    // updating in inspector nogthing more
    public bool autoUpdate;
    [Header("Height settings")]
    public int heightOctaves;
    public float heightFrequency = 1f;
    public float heightExp;

    [Header("Temperature settings")]
    public float temperatureMultiplier; //increasing this will make poles move more further to equator ->default 1.0f
     public float temperatureLoss; //temperature loss for each height increase
    [Header("Precipitation settings")]
    public int precipitationOctaves;
    public float precipitationPersistance;
    public float precipitationLacunarity;
    [Header("Objects (trees etc)")]
    public float treeScale;

    public int renderDistance = 42;    //distance to load chunk
    //chunks stuff
    private Dictionary<int2, WorldChunk> chunks = new Dictionary<int2, WorldChunk>(); //map
    private Dictionary<int2, GameObject> renderedChunks = new Dictionary<int2, GameObject>();
    
    /// <summary>
    ///  This function handles whole map creation process.Three dictionaries each holding chunks of different types
    ///  are being used to determine which biome and what exact type should be picked for each coodinate of map.
    /// </summary>
    public void MapGeneration(){
        /* for inspector because global initialization does not affect inspector in this case */
        chunks = new Dictionary<int2, WorldChunk>();

        /* remove later */
        //initialization for inspector mode
        if (chunkGenerator == null) chunkGenerator = GetComponent<ChunkGenerator>();

        /* get perlin values in advance ( doing this when loading chunks is irrelevant generating values is fast ) */
        for (int x = 0; x < width; x+=chunkSize)
        {
            for (int y = 0; y < height; y+=chunkSize)
            {

                chunks = chunkGenerator.GenerateChunks(     x,y,chunks,
                                                            seed, heightSeed, precipitationSeed,
                                                            scale, heightOctaves, precipitationOctaves,
                                                            heightFrequency, precipitationPersistance, temperatureMultiplier,
                                                            heightExp, precipitationLacunarity, temperatureLoss,
                                                            new int2(width,height), treeScale);
            }
        }

        /* loading all chunks */
        //all at once (only during testing remove later.)
        /*if (!chunkLoading){
            foreach ( var chunk in chunks )
            {
                //create chunk object
                GameObject chunkP = Instantiate(chunkPrefab, new Vector3(0,0,0), Quaternion.identity);
                chunkP.transform.parent = gameObject.transform;
                ChunkCreator chunkCreator = chunkP.GetComponent<ChunkCreator>(); //reference to script
                //create mesh (chunk) and save it to structure holding chunk
                chunks[chunk.Key].chunkMesh = chunkCreator.CreateTileMesh(chunkSize,chunkSize, chunk.Key.x, chunk.Key.y);
            }
        }*/
    }

    /// <summary>
    /// Loop through all chunks, and determine whenever chunk should be loaded or 
    /// unloaded by calculating distance and checking treshold.
    /// </summary>
    /// <param name="PlayerPos">Position of player in the world</param>
    /// <param name="distToLoad">Distance treshold to load chunk</param>
    /// <param name="distToUnload">Distance treshold to unload chunk</param>
    public void LoadChunks(Vector3 PlayerPos, float distToLoad, float distToUnload){
        int offset = 16; //chunk 32 -> 16, 16 is center
		for(int x = 0; x < width; x+=chunkSize){
			for(int y = 0; y < height ; y+=chunkSize){
                //calcualte distance to the middle of chunk
                int chunkX = x + offset;
                int chunkY = y + offset;
				float dist=Vector2.Distance(new Vector2(chunkX,chunkY),new Vector2(PlayerPos.x,PlayerPos.y));
                if(dist<distToLoad){
                    if(!renderedChunks.ContainsKey(new int2(x,y))){
						CreateChunk(x,y);
					}
				}else if(dist>distToUnload){
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
        //reference to the script, attached to chunk about to remove
        ChunkCreator chunkCreator = renderedChunks[new int2(x,y)].GetComponent<ChunkCreator>(); 
        chunkCreator.UnloadTrees();
        //deactivate chunk
        renderedChunks[new int2(x,y)].SetActive(false);
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
        GameObject chunkP = ChunkPool.instance.GetPooledObject();
        if(chunkP != null){
            chunkP.transform.parent = gameObject.transform;
            chunkP.SetActive(true);
            ChunkCreator chunkCreator = chunkP.GetComponent<ChunkCreator>(); //reference to script
            //create mesh (chunk) and save it to structure holding chunk
            chunks[new int2(x,y)].chunkMesh = chunkCreator.CreateTileMesh(chunkSize,chunkSize, x, y, renderDistance);
            renderedChunks.Add(new int2(x,y), chunkP);
        }
    }

    /// <summary>
    /// Height getter
    /// </summary>
    /// <param name="key">Chunk key</param>
    /// <param name="x">x coord</param>
    /// <param name="y">y coord</param>
    /// <returns>Height of tile.</returns>
    public float GetHeightValue(int2 key, int x, int y){
        return chunks[key].sample[x,y].height;
    }
    /// <summary>
    /// Moisture getter
    /// </summary>
    /// <param name="key">Chunk key</param>
    /// <param name="x">x coord</param>
    /// <param name="y">y coord</param>
    /// <returns>Moisture of tile.</returns>
    public float GetPrecipitationValue(int2 key, int x, int y){
        return chunks[key].sample[x,y].precipitation;
    }
    /// <summary>
    /// Temperature getter
    /// </summary>
    /// <param name="key">Chunk key</param>
    /// <param name="x">x coord</param>
    /// <param name="y">y coord</param>
    /// <returns>Temperature of tile.</returns>
    public float GetTemperatureValue(int2 key, int x, int y){
        return chunks[key].sample[x,y].temperature;
    }

    /// <summary>
    /// Finds closes neighbours in 4 directions (bot, top, right, left) and sets reference for each tile to them.
    /// If tile is on the edge of the map, references itself. (Similiar concept to linked list)
    /// </summary>
    /// <param name="tile">TDTile structure holding information about single tile</param>
    /// <param name="chunkPos">Key position of chunk (bottom left corner of chunk)</param>
    /// <returns>Properly set tile with corresponding edge types.</returns>
    public TDTile AssignNeighbours(TDTile tile, int2 chunkPos){
        int2 leftTilePos = new int2(tile.pos.x - 1, tile.pos.y);
        int2 topLeftTilePos = new int2(tile.pos.x - 1, tile.pos.y + 1);
        int2 topTilePos = new int2(tile.pos.x, tile.pos.y + 1);
        int2 topRightTilePos = new int2(tile.pos.x+1, tile.pos.y + 1);
        int2 rightTilePos = new int2(tile.pos.x + 1, tile.pos.y);
        int2 botRightTilePos = new int2(tile.pos.x+1, tile.pos.y - 1);
        int2 botTilePos = new int2(tile.pos.x, tile.pos.y - 1);
        int2 botLeftTilePos = new int2(tile.pos.x - 1, tile.pos.y - 1);

        bool leftOverflow = false;
        bool rightOverflow = false;
        bool topOverflow = false;
        bool botOverflow = false;

        if (leftTilePos.x < chunkPos.x)  leftOverflow = true;
        if (rightTilePos.x >= chunkPos.x + chunkSize) rightOverflow = true;
        if (topTilePos.y >= chunkPos.y + chunkSize) topOverflow = true;
        if (botTilePos.y < chunkPos.y) botOverflow = true;
        
        int2 relativePos = TileRelativePos(new int2(tile.pos.x, tile.pos.y));
        //int2 chunkKey = TileChunkPos(new int2());
        /*--------------------------------------
            Assigning neighbours to each tile.
        ---------------------------------------*/
        /* assign left neighbour */
        tile.left   = SetNeighbours(leftTilePos,  tile.pos.x>0,           leftOverflow,   "left");
        tile.right  = SetNeighbours(rightTilePos, tile.pos.x != width-1,  rightOverflow,  "right");
        tile.top    = SetNeighbours(topTilePos,   tile.pos.y != height-1, topOverflow,    "top");
        tile.bottom = SetNeighbours(botTilePos,   tile.pos.y >0,          botOverflow,    "bot");


        /*-----------------------------------------
        corners topLeft, topRight, botLeft, botRight 
        ------------------------------------------*/
        tile.bottomLeft = SetNeighbours(botLeftTilePos,true,                                              leftOverflow && botOverflow,   "botLeft");
        tile.bottomRight= SetNeighbours(botRightTilePos, tile.pos.y != 0 && tile.pos.x != width-1,        botOverflow && rightOverflow,  "botRight");
        tile.topLeft    = SetNeighbours(topLeftTilePos, tile.pos.y != height-1,                           leftOverflow && topOverflow,    "topLeft");
        tile.topRight   = SetNeighbours(topRightTilePos, tile.pos.y != height-1 && tile.pos.x != width-1, topOverflow && rightOverflow,    "topRight");

        /* stuff for choosing correct texture for edges of biomes */
        bool isLeftSame, isTopLeftSame, isTopSame, isTopRightSame, isRightSame, isBotRightSame, isBotSame, isBotLeftSame;
        TextureEdgesConditions(tile,out isLeftSame, out isTopLeftSame, out isTopSame, out isTopRightSame, out isRightSame, out isBotRightSame, out isBotSame, out isBotLeftSame,false);
        tile = TileEdgeType(tile, isLeftSame,  isTopSame,  isRightSame,  isBotSame, isBotLeftSame,  isTopLeftSame,  isTopRightSame,  isBotRightSame, false);

        /* stuff for choosing correct texture for hills transitions */
        TextureEdgesConditions(tile,out isLeftSame, out isTopLeftSame, out isTopSame, out isTopRightSame, out isRightSame, out isBotRightSame, out isBotSame, out isBotLeftSame, true);
        tile = TileEdgeType(tile, isLeftSame,  isTopSame,  isRightSame,  isBotSame, isBotLeftSame,  isTopLeftSame,  isTopRightSame,  isBotRightSame, true);

        return tile;
    }


    /// <summary>
    /// Abstract function, that handles setting correct neighbour for given tile.
    /// </summary>
    /// <param name="neighbour">coords of neighbour tile</param>
    /// <param name="condition">condition that for edges of the map</param>
    /// <param name="overflow">does neighbour overflows to next chunk?</param>
    /// <param name="side">which neighbour we are dealing with</param>
    /// <returns>Returns tile with correctly set neighbour.</returns>
    private TDTile SetNeighbours(int2 neighbour, bool condition, bool overflow, string side){
        /*edgy coords must be handled */
        if (neighbour.x == -1) neighbour.x = 0;
        if (neighbour.x >= width) neighbour.x = width-1;
        if (neighbour.y == -1) neighbour.y = 0;
        if (neighbour.y >= height) neighbour.y = height-1;

        TDTile tile = AssignNeighbour(neighbour);
        
        return tile;
    }

    /// <summary>
    ///  Calcualtes neighbour according to given position
    /// </summary>
    /// <param name="neighbourPos">Position of neighbour tile within map</param>
    /// <returns>Neighbour tile with set biome height etc.</returns>
    private TDTile AssignNeighbour(int2 neighbourPos){
        int2 chunkKey = TileChunkPos(neighbourPos);
        int2 relativePos = TileRelativePos(neighbourPos);
        TDTile tile = chunks[chunkKey].sample[relativePos.x, relativePos.y];

        float elevation = GetHeightValue(chunkKey,relativePos.x,relativePos.y);
        float moisture = GetPrecipitationValue(chunkKey,relativePos.x,relativePos.y);
        float temperature = GetTemperatureValue(chunkKey,relativePos.x,relativePos.y);
        tile.biome = GetBiome(elevation, moisture, temperature, chunkKey, relativePos);

        return tile;
    }

    /// <summary>
    /// Transfers given world map position @var absoluteCoords into position within chunk.
    /// </summary>
    /// <param name="absoluteCoords"></param>
    /// <returns>int2 relative coords of given absolute coords.</returns>
    public int2 TileRelativePos(int2 absoluteCoords){
        int x = absoluteCoords.x % 32;
        int y =  absoluteCoords.y % 32;
        return new int2(x,y); //tile position relative to chunk
    }

    /// <summary>
    /// From given tile coords, determine what are coords of chunk where tile belongs to.
    /// </summary>
    /// <param name="absoluteCoords"></param>
    /// <returns>Chunk key int2.</returns>
    public int2 TileChunkPos(int2 absoluteCoords){
        int2 relative = TileRelativePos(absoluteCoords);
        int x = absoluteCoords.x - relative.x;
        int y = absoluteCoords.y - relative.y;

        return new int2(x,y); //tile position relative to chunk
    }

    /// <summary>
    /// Returns reference to tile(TDTile class) according to given coordinates.
    /// </summary>
    /// <param name="relativePos"> position of tile within chunk</param>
    /// <param name="chunkPos">key of chunk (bottom left tile of chunk 32x32)</param>
    /// <returns></returns>
    public TDTile GetTile(int2 relativePos, int2 chunkPos){
        return chunks[chunkPos].sample[relativePos.x, relativePos.y];
    }

    /// <summary>
    /// Retrieve chunk based on given key
    /// </summary>
    /// <param name="chunkKey">key of chunk</param>
    /// <returns>Requested chunk</returns>
    public WorldChunk GetChunk(int2 chunkKey){
        return chunks[chunkKey];
    }

    /// <summary>
    /// Check 8 different tiles around given tile whenever they are same as @tile or not. With
    /// given parameter_(string) of specific biome, only this biome is being taken into account.
    /// </summary>
    /// <param name="tile">Tile reference</param>
    /// <param name="isLeftSame">Is left tile same biome</param>
    /// <param name="isTopLeftSame">Is top-left tile same biome</param>
    /// <param name="isTopSame">Is top tile same biome</param>
    /// <param name="isTopRightSame">Is top-right tile same biome</param>
    /// <param name="isRightSame">Is right tile same biome</param>
    /// <param name="isBotRightSame">Is bot right tile same biome</param>
    /// <param name="isBotSame">Is bottom tile same biome</param>
    /// <param name="isBotLeftSame">Is left bot-left same biome</param>
    /// <param name="zLevel">Is Z-index same.</param>
    /// <returns>false if everything around is basically the same, true otherwise.</returns>
    public bool TextureEdgesConditions(TDTile tile, out bool isLeftSame, out bool isTopLeftSame, out bool isTopSame, out bool isTopRightSame, out bool isRightSame, out bool isBotRightSame, out bool isBotSame, out bool isBotLeftSame, bool zLevel){
        //compare Z level of tiles
        if (zLevel)
        {
            isLeftSame     = (tile.z_index <= tile.left.z_index) ? true : false;
            isTopLeftSame  = (tile.z_index <= tile.topLeft.z_index) ? true : false;
            isTopSame      = (tile.z_index <= tile.top.z_index) ? true : false;
            isTopRightSame = (tile.z_index <= tile.topRight.z_index) ? true : false;
            isRightSame    = (tile.z_index <= tile.right.z_index) ? true : false;
            isBotRightSame = (tile.z_index <= tile.bottomRight.z_index) ? true : false;
            isBotSame      = (tile.z_index <= tile.bottom.z_index) ? true : false;
            isBotLeftSame  = (tile.z_index <= tile.bottomLeft.z_index) ? true : false;
        //compare with all biomes
        }else{
            isLeftSame     = tile.biome.type.Equals(tile.left.biome.type) ? true : false;
            isTopLeftSame  = tile.biome.type.Equals(tile.topLeft.biome.type) ? true : false;
            isTopSame      = tile.biome.type.Equals(tile.top.biome.type) ? true : false;
            isTopRightSame = tile.biome.type.Equals(tile.topRight.biome.type) ? true : false;
            isRightSame    = tile.biome.type.Equals(tile.right.biome.type) ? true : false;
            isBotRightSame = tile.biome.type.Equals(tile.bottomRight.biome.type) ? true : false;
            isBotSame      = tile.biome.type.Equals(tile.bottom.biome.type) ? true : false;
            isBotLeftSame  = tile.biome.type.Equals(tile.bottomLeft.biome.type) ? true : false;
        }
        //no neighbour in any direction with given biome, this is required for determining biome wthin continent, not ocean
        if (isLeftSame && isTopLeftSame && isRightSame && isTopRightSame && isBotRightSame && isBotSame && isBotLeftSame)
            return false;
        else
            return true;
    }


    /// <summary>
    ///  Assigns what type of edge tile given tile is. Whenever it is left only, right only, corner etc.
    ///  This is used to determine edgy textures later (mostly during generation of mountains).
    /// </summary>
    /// <param name="tile">Tile reference</param>
    /// <param name="isLeftSame">Is left tile same biome</param>
    /// <param name="isTopLeftSame">Is top-left tile same biome</param>
    /// <param name="isTopSame">Is top tile same biome</param>
    /// <param name="isTopRightSame">Is top-right tile same biome</param>
    /// <param name="isRightSame">Is right tile same biome</param>
    /// <param name="isBotRightSame">Is bot right tile same biome</param>
    /// <param name="isBotSame">Is bottom tile same biome</param>
    /// <param name="isBotLeftSame">Is left bot-left same biome</param>
    /// <param name="zLevel">Is Z-index same.</param>
    /// <returns>Tile with correctly determined edging tiles.</returns>
    private TDTile TileEdgeType(TDTile tile, bool IsLeftSame, bool IsTopSame, bool IsRightSame, bool IsBotSame,bool IsBotLeftSame, bool IsTopLeftSame, bool IsTopRightSame, bool IsBotRightSame, bool zLevel){
        bool leftOnly   = !IsLeftSame    && IsTopSame    && IsRightSame     && IsBotSame; 
        bool rightOnly  = IsLeftSame    && IsTopSame   && !IsRightSame     && IsBotSame; 
        bool topOnly    = IsLeftSame    && !IsTopSame    && IsRightSame    && IsBotSame; 
        bool botOnly    = IsLeftSame    && IsTopSame   && IsRightSame    && !IsBotSame; 
        bool botLeft    = !IsLeftSame     && IsTopSame   && IsRightSame    && !IsBotSame;
        bool topLeft    = !IsLeftSame     && !IsTopSame    && IsRightSame    && IsBotSame;
        bool topRight   = IsLeftSame    && !IsTopSame    && !IsRightSame     && IsBotSame;
        bool botRight   = IsLeftSame    && IsTopSame   && !IsRightSame     && !IsBotSame;
        //corners
        bool botLeftOnly =  IsLeftSame     && IsTopSame   && IsRightSame    && IsBotSame && !IsBotLeftSame;
        bool topLeftOnly =  IsLeftSame     && IsTopSame   && IsRightSame    && IsBotSame && !IsTopLeftSame;
        bool topRightOnly = IsLeftSame     && IsTopSame   && IsRightSame    && IsBotSame && !IsTopRightSame;
        bool botRightOnly = IsLeftSame     && IsTopSame   && IsRightSame    && IsBotSame && !IsBotRightSame;
        //rare occurences
        bool rareTRB = IsLeftSame    && !IsTopSame    && !IsRightSame     && !IsBotSame;
        bool rareLTR = !IsLeftSame    && !IsTopSame    && !IsRightSame     && IsBotSame;
        bool rareRBL = !IsLeftSame    && IsTopSame    && !IsRightSame     && !IsBotSame;
        bool rareBLT = !IsLeftSame    && !IsTopSame    && IsRightSame     && !IsBotSame;
        bool rareTB = !IsLeftSame    && IsTopSame    && !IsRightSame     && IsBotSame;

        if (zLevel)
        {
            tile.hillEdge = EdgeType.none;
        }else{
            tile.edgeType = EdgeType.none;
        }
        
        /* regular edged tiles */
        if (leftOnly){
            if (zLevel) tile.hillEdge = EdgeType.left;
            else        tile.edgeType = EdgeType.left; 
        }else if (rightOnly){
            if (zLevel) tile.hillEdge = EdgeType.right;
            else tile.edgeType = EdgeType.right;
        }else if (topOnly){
            if (zLevel) tile.hillEdge = EdgeType.top;
            else tile.edgeType = EdgeType.top;
        }else if (botOnly){
            if (zLevel) tile.hillEdge = EdgeType.bot;
            else tile.edgeType = EdgeType.bot;
        }/*two sides*/
        else if (botLeft){
            if (zLevel) tile.hillEdge = EdgeType.botLeft;
            else tile.edgeType = EdgeType.botLeft;
        }else if (topLeft){
            if (zLevel) tile.hillEdge = EdgeType.topLeft;
            else tile.edgeType = EdgeType.topLeft;
        }else if (topRight){
            if (zLevel) tile.hillEdge = EdgeType.topRight;
            else tile.edgeType = EdgeType.topRight;
        }else if (botRight){
            if (zLevel) tile.hillEdge = EdgeType.botRight;
            else tile.edgeType = EdgeType.botRight;
        /*corners*/
        }else if (botLeftOnly){
            if (zLevel) tile.hillEdge = EdgeType.botLeftOnly;
            else tile.edgeType = EdgeType.botLeftOnly;
        }else if (topLeftOnly){
            if (zLevel) tile.hillEdge = EdgeType.topLeftOnly;
            else tile.edgeType = EdgeType.topLeftOnly;
        }else if (topRightOnly){
            if (zLevel) tile.hillEdge = EdgeType.topRightOnly;
            else tile.edgeType = EdgeType.topRightOnly;
        }else if (botRightOnly){
            if (zLevel) tile.hillEdge = EdgeType.botRightOnly;
            else tile.edgeType = EdgeType.botRightOnly;
        /*rare occurences*/
        //TRB -> top, right,bottom surrounded tile
        }else if (rareTRB){
            //if (zLevel) tile.hillEdge = EdgeType.rareTRB;
            tile.edgeType = EdgeType.rareTRB;
        }else if (rareLTR){
            //if (zLevel) tile.hillEdge = EdgeType.rareLTR;
            tile.edgeType = EdgeType.rareLTR;
        }else if (rareRBL){
            //if (zLevel) tile.hillEdge = EdgeType.rareRBL;
            tile.edgeType = EdgeType.rareRBL;
        }else if (rareBLT){
            //if (zLevel) tile.hillEdge = EdgeType.rareBLT;
            tile.edgeType = EdgeType.rareBLT;
        }else if (rareTB){
            //if (zLevel) tile.hillEdge = EdgeType.rareTB;
            tile.edgeType = EdgeType.rareTB;
        }

        return tile;
    }

    /// <summary>
    /// When generating terrain, malformations may occure. Especially 
    /// during creating mountains. If mountain is only 3 tiles high,
    /// that means it cannot by properly closed by top edge. This function
    /// detect 3 tiles heigh parts of mountains, and eliminates them.
    /// </summary>
    /// <param name="tile">Tile reference</param>
    /// <returns>Z-index of tile</returns>
    private int TrimTerrainMalformations(TDTile tile){
        int ret_z_index = tile.z_index;
        int y_pos = tile.pos.y+3;
        int2 relativePos = TileRelativePos(new int2(tile.pos.x, y_pos));
        int2 chunkPos = TileChunkPos(new int2(tile.pos.x, y_pos));
        TDTile nextTile = GetTile(relativePos, chunkPos);
        
        y_pos = ((tile.pos.y-1) >= 0 ) ? tile.pos.y-1 : 0;
        
        relativePos = TileRelativePos(new int2(tile.pos.x, y_pos));
        chunkPos = TileChunkPos(new int2(tile.pos.x,y_pos));
        TDTile prevTile = GetTile(relativePos, chunkPos);
        if (prevTile.z_index != tile.z_index && prevTile.z_index < tile.z_index)
        {
            if(nextTile.z_index != tile.z_index){
                ret_z_index = prevTile.z_index;
            }
        }

        return ret_z_index;
    }
    /// <summary>
    /// Based on given elevation determines what biome should be assigned to
    /// given coordinates. If no conditions are matched, send moisture and temperature
    /// to GetClosestBiome function to determine biome by euclidean distance.
    /// </summary>
    /// <param name="elevation">Perlin elevation value of tile</param>
    /// <param name="moisture">Perlin moisture value of tile</param>
    /// <param name="temperature">Perlin heat value of tile</param>
    /// <param name="key">Key of chunk where tile belongs to.</param>
    /// <param name="tile_coords">Absolute coordinates of tile.</param>
    /// <returns>Biome for tile</returns>
    public BiomePreset GetBiome(float elevation, float moisture, float temperature, int2 key, int2 tile_coords){
        BiomePreset biomeToReturn = null;
        TDTile tile = chunks[key].sample[tile_coords.x, tile_coords.y];

        if (tile_coords.y + 3 < chunkSize)
        {
            tile.z_index = TrimTerrainMalformations(tile);
        }

        /* water biomes are determined purely on height*/
        if (elevation < 0.08){
            //landmass water (lakes etc.)
            if (tile.landmass == true)
            {
                tile.biome = biomes[6];
                return biomes[6];
            }else{
                tile.biome = biomes[4];
                return biomes[4];
            }
        }
        /* beach for  smoother transition from water to landmass*/
        else if (elevation <= 0.25 && !tile.landmass){
            tile.biome = biomes[5];
            return biomes[5]; 
        }

        //after height based biomes (water / beach) choose correct according to conditions
        biomeToReturn = GetClosestBiome(temperature, moisture, key, tile_coords);
        tile.biome = biomeToReturn;

        return biomeToReturn;
    }
    /// <summary>
    /// Calculates euclidean distance to every biome excepy beach and ocean. Closest
    /// one is determined to be correct one.
    /// </summary>
    /// <param name="temperature">Temperature value of tile</param>
    /// <param name="precipitation">Moisture value of tile</param>
    /// <param name="key">Key of chunk</param>
    /// <param name="tile_coords">Absolute coords of tile</param>
    /// <returns>Biome for tile at tile coords.</returns>
    private BiomePreset GetClosestBiome(float temperature, float precipitation, int2 key, int2 tile_coords){
        List<BiomePreset> localBiomes = new List<BiomePreset>(biomes);
        
        localBiomes.RemoveAll(b => b.type=="ocean" || b.type =="beach");
        
        BiomePreset biomeToReturn = null;
        Dictionary<float, BiomePreset> dict = new Dictionary<float, BiomePreset>();

        float minDistance = 0f;
        foreach (BiomePreset biome in localBiomes){
            float euclideanDistance = biome.EuclideanDistance(temperature,precipitation); //get euclidean distance between tile and biome
            dict.Add(euclideanDistance,biome);

            if (minDistance == 0f) minDistance = euclideanDistance;
            else if (minDistance > euclideanDistance) minDistance = euclideanDistance;
        }
        biomeToReturn = dict[minDistance];
        chunks[key].sample[tile_coords.x, tile_coords.y].biome = biomeToReturn;
        return biomeToReturn;
    }
    

    /* * * * * * * * * * * * * * * ** 
    
    
    Debbuging functions( inspector )


    * * * * * * * * * * * * * * * * */

    public void DestroyChildPrefabs(){
        foreach(Transform child in this.transform)
        {
            DestroyImmediate(child.gameObject);
        }
        if (this.transform.childCount > 0){
            DestroyChildPrefabs();
        }
    }

    void Start(){
        chunkGenerator = GetComponent<ChunkGenerator>();
        MapGeneration(); //generate map
    }

    void Update(){
        if (chunkLoading)
        {
            LoadChunks(player.transform.position, renderDistance,renderDistance + 15);
        }
    }
    
}
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using System;


/// <summary>
/// This Class is handling creation of each chunk. Mesh is created on whose is applied texture representing tiles.
/// This improves performance a big amount.
/// </summary>
public class ChunkCreator : MonoBehaviour
{
    MeshRenderer meshRenderer;
    MeshFilter meshFilter;
    Map mapReference;
    ChunkLoader chunkLoader;
    TreePool treePool;
    GameHandler gameHandler;
    WorldChunk chunk;
    private float tileSize = 1;
    public Vector3[] vertices;
    public Vector2[] uv;
    public int[] triangles;
    public TDTile[,] tiles;
    public int x;
    public int top_x;
    public int y;
    public int top_y;
    private Dictionary<int2, GameObject> renderedTrees = new Dictionary<int2, GameObject>();
    [SerializeField] public Sprite testSprite;
    private bool treesLoaded = false;

    /// <summary>
    /// Function that creates mesh with given width, height and world space. This tile represents
    /// one chunk. Mesh is divided into 32 x 32 quads. Each quad represents 1 world tile. Each tile's
    /// UV is set accordingly to match texture desired. Consists of loop throught the map, that
    /// is creating quads for tiles and setting it's biomes, and sets uvs and texturing accordingly.
    /// </summary>
    /// <param name="width">chunk width</param>
    /// <param name="height">chunk height</param>
    /// <param name="chunkX">key x coord(bot left)</param>
    /// <param name="chunkY">key y coord(bot left)</param>
    /// <returns></returns>
    public Mesh CreateTileMesh(WorldChunk chunk, Mesh mesh)
    {
        this.chunk = chunk;
        x = chunk.position.x;
        y = chunk.position.y;
        top_x = x + 32;
        top_y = y + 32;


        //check for passed cached chunk data
        this.vertices = new Vector3[4 * (Const.CHUNK_SIZE * Const.CHUNK_SIZE)];
        this.uv = new Vector2[4 * (Const.CHUNK_SIZE * Const.CHUNK_SIZE)];
        this.triangles = new int[6 * (Const.CHUNK_SIZE * Const.CHUNK_SIZE)];

        for (int x = 0; x < Const.CHUNK_SIZE; x++)
        {
            for (int y = 0; y < Const.CHUNK_SIZE; y++)
            {
                int index = x * Const.CHUNK_SIZE + y;
                vertices[index * 4 + 0] = new Vector3(tileSize * x + chunk.position.x, tileSize * y + chunk.position.y);
                vertices[index * 4 + 1] = new Vector3(tileSize * x + chunk.position.x, tileSize * (y + 1) + chunk.position.y);
                vertices[index * 4 + 2] = new Vector3(tileSize * (x + 1) + chunk.position.x, tileSize * (y + 1) + chunk.position.y);
                vertices[index * 4 + 3] = new Vector3(tileSize * (x + 1) + chunk.position.x, tileSize * y + chunk.position.y);

                triangles[index * 6 + 0] = index * 4 + 0;
                triangles[index * 6 + 1] = index * 4 + 1;
                triangles[index * 6 + 2] = index * 4 + 2;

                triangles[index * 6 + 3] = index * 4 + 0;
                triangles[index * 6 + 4] = index * 4 + 2;
                triangles[index * 6 + 5] = index * 4 + 3;

                // now set UVs and textures accordingly
                Sprite tileSprite = GetTileTexture(x, y, chunk.position);

                //map UVs for each tile to specific texture in atlas
                this.uv[index * 4 + 0] = SetTileTexture(0, tileSprite);
                this.uv[index * 4 + 1] = SetTileTexture(1, tileSprite);
                this.uv[index * 4 + 2] = SetTileTexture(2, tileSprite);
                this.uv[index * 4 + 3] = SetTileTexture(3, tileSprite);
                
                
                /* 
                DEPRECATED
                 doing this in MapGeneration now
                set biome for each tile, and pointers to 8 direction neighbours
                */
                //SetTileBiome(x, y, chunk.position);
            }
        }

        /*
        DEPRECATED
         now set UVs and textures accordingly */
        /*
        for (int x = 0; x < Const.CHUNK_SIZE; x++)
        {
            for (int y = 0; y < Const.CHUNK_SIZE; y++)
            {
                int index = x * Const.CHUNK_SIZE + y;
                Sprite tileSprite = GetTileTexture(x, y, chunk.position);

                //map UVs for each tile to specific texture in atlas
                this.uv[index * 4 + 0] = SetTileTexture(0, tileSprite);
                this.uv[index * 4 + 1] = SetTileTexture(1, tileSprite);
                this.uv[index * 4 + 2] = SetTileTexture(2, tileSprite);
                this.uv[index * 4 + 3] = SetTileTexture(3, tileSprite);
            }
        }*/

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        meshFilter.mesh = mesh;

        this.chunk.chunkMesh = mesh;
        return mesh;
    }

    /// <summary>
    /// Sets tile's neighbourhood(pointers in 8 directions) and sets it's biome accordingly.
    /// </summary>
    /// <param name="x">x coord</param>
    /// <param name="y">y coord</param>
    /// <param name="chunkX">chunk x key</param>
    /// <param name="chunkY">chunk y key</param>
    private void SetTileBiome(int x, int y, int2 key)
    {
        //bool isLeftSame, isTopLeftSame, isTopSame, isTopRightSame, isRightSame, isBotRightSame, isBotSame, isBotLeftSame, sameAround = false;
        mapReference.AssignNeighbours(mapReference.GetTile(new int2(x, y), key), key);
    }

    /// <summary>
    /// Return texture according to given coordinatite
    /// </summary>
    /// <param name="x">x coord</param>
    /// <param name="y">y coord</param>
    /// <param name="chunkX">chunk x key</param>
    /// <param name="chunkY">chunk y key</param>
    /// <returns>Texture for tile</returns>
    private Sprite GetTileTexture(int x, int y, int2 key)
    {
        // get reference to tile working with
        TDTile tile = mapReference.GetTile(new int2(x, y), key);
        //set material texture so we can assign uvs later on 
        SetMaterialTexture(tile.biome.GetRandomSprite().texture);

        Sprite ret = tile.biome.GetTileSprite(tile);
        return ret;
    }

    /// <summary>
    /// Sets UV's for quads within mesh for each tile. 
    /// </summary>
    /// <param name="corner">Corner of quad that is being processed.</param>
    /// <param name="tileSprite">Sprite holding texture</param>
    /// <returns>Vector2 UV for given corner of quad.</returns>
    private Vector2 SetTileTexture(int corner, Sprite tileSprite)
    {
        Rect UVs = tileSprite.rect;
        UVs.x /= tileSprite.texture.width;
        UVs.width /= tileSprite.texture.width;
        UVs.y /= tileSprite.texture.height;
        UVs.height /= tileSprite.texture.height;
        /*
            Return UV for each corner of texture within atlas
        */
        Vector2 retval = new Vector2(0, 0);
        switch (corner)
        {
            case 0: //bottom left
                retval = new Vector2(UVs.x, UVs.y);
                break;
            case 1: //top left
                retval = new Vector2(UVs.x, UVs.y + UVs.height);
                break;
            case 2: //top right
                retval = new Vector2(UVs.x + UVs.width, UVs.y + UVs.height);
                break;
            case 3: //bottom right
                retval = new Vector2(UVs.x + UVs.width, UVs.y);
                break;
        }

        return retval;
    }
    /// <summary>
    /// Set material texture holding all tile textures to be used in meshes.
    /// </summary>
    /// <param name="texture">Tiles texture</param>
    private void SetMaterialTexture(Texture2D texture)
    {
        meshRenderer.sharedMaterials[0].mainTexture = texture;
        return;
    }

    /// <summary>
    /// Spawn trees in loaded chunk
    /// </summary>
    /// <param name="x">x coordinate</param>
    /// <param name="y">y coordinate</param>
    /// <param name="chunkX">Chunk x key</param>
    /// <param name="chunkY">Chunk y key</param>
    private void LoadTrees()
    {
        WorldChunk loaded = LoadChunk();
        //TreePool treePool = GetComponent<TreePool>();
        for (int x = 0; x < Const.CHUNK_SIZE; x++)
        {
            for (int y = 0; y < Const.CHUNK_SIZE; y++)
            {
                int2 relativePos = new int2(x, y);
                TDTile tile = mapReference.GetTile(relativePos, chunk.position);
                if (chunk.treeMap[x, y] == 1
                && tile.biome.type != "ocean"
                && tile.biome.type != "water"
                && tile.hillEdge == EdgeType.none
                && tile.edgeType == EdgeType.none)
                {
                    int x_coord = chunk.position.x + x;
                    int y_coord = chunk.position.y + y;
                    int2 actualPos = new int2(x_coord, y_coord);
                    GameObject treeP = treePool.GetPooledObject();
                    if (treeP != null && TreeRadius(actualPos))
                    {
                        treeP.transform.parent = gameObject.transform;
                        treeP.transform.position = new Vector3(chunk.position.x + x, chunk.position.y + y, 0);
                        treeP.SetActive(true);
                        //set correct sprite and save
                        Sprite treeSprite;
                        if(loaded == null){
                            treeSprite = SetTreeSprite(treeP, tile);
                            //mark picked sprite to chunk
                            chunk.trees.Add(treeSprite.name);
                        }else{
                            //pop first sprite from json and assign to tree
                            treeSprite = SetTreeSprite(treeP, tile, loaded.trees[0]);
                            if (loaded.trees.Count != 0)
                            {
                                loaded.trees.RemoveAt(0);
                            };
                        }
                        renderedTrees[actualPos] = treeP;
                        AdjustTreeCollider(treeP.GetComponent<CapsuleCollider2D>(), treeSprite.bounds, treeSprite.name);

                    }
                }
            }
        }
        treesLoaded = true;
        if (loaded == null){
            SaveChunk();
        }
        
        return;
    }

    /// <summary>
    /// Unload all trees and clear renderedTree list.
    /// </summary>
    public void UnloadTrees()
    {
        foreach (GameObject tree in renderedTrees.Values)
        {
            tree.SetActive(false);
        }
        renderedTrees.Clear();
        treesLoaded = false;
    }

    private void SaveChunk(){
        gameHandler.Save<WorldChunk>(this.chunk, ObjType.Chunk, new Vector3(this.x, this.y, 0));
    }

    private WorldChunk LoadChunk(){
        return gameHandler.Load<WorldChunk>(ObjType.Chunk,this.x, this.y);
    }

    /// <summary>
    /// Check if tree about to spawn can be actually spawner. If another tree is already
    /// spawned in radius 2, tree wort spawn. Tree radius from chunk generator works, however
    /// multiple trees spawn on 1 location. This function handles that problem
    /// </summary>
    /// <param name="coords">Coords of root points.</param>
    /// <returns>Boolean value whenever tree can or can not be spawned.</returns>
    private bool TreeRadius(int2 coords)
    {
        foreach (int2 tree in renderedTrees.Keys)
        {
            if (EuclideanDistance(coords, tree) < 4f)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Calculates distance between two points.
    /// </summary>
    /// <param name="p1">Point 1</param>
    /// <param name="p2">Point 2</param>
    /// <returns>Float distance between them.</returns>
    private float EuclideanDistance(int2 p1, int2 p2)
    {
        Vector2 v1 = new Vector2(p1.x, p1.y);
        Vector2 v2 = new Vector2(p2.x, p2.y);
        return Vector2.Distance(v1, v2);
    }

    /// <summary>
    /// Set correct tree sprite for given tile and edit given gameobject to that.
    /// </summary>
    /// <param name="tree">Tree gameobject</param>
    /// <param name="tile">Processed tile</param>
    /// <returns>Sprite that was assigned to tree</returns>
    public Sprite SetTreeSprite(GameObject tree, TDTile tile, string loaded = null)
    {
        TreeCreator treeCreator = tree.GetComponent<TreeCreator>();
        SpriteRenderer treeRenderer = tree.GetComponent<SpriteRenderer>();
        switch (tile.biome.type)
        {
            case "forest":
                if (loaded!=null)
                {
                    treeRenderer.sprite = treeCreator.GetForestTree(loaded);
                }else{
                    treeRenderer.sprite = treeCreator.GetRandomForestTree();
                }
                break;
            case "ashland":
                if (loaded!=null)
                {
                    treeRenderer.sprite = treeCreator.GetAshlandTree(loaded);
                }else{
                    treeRenderer.sprite = treeCreator.GetRandomAshlandTree();
                }
                break;
            case "rainforest":
                if (loaded!=null)
                {
                    treeRenderer.sprite = treeCreator.GetJungleTree(loaded);
                }else{
                    treeRenderer.sprite = treeCreator.GetRandomJungleTree();
                }
                break;
            case "beach":
                if (loaded!=null)
                {
                    treeRenderer.sprite = treeCreator.GetBeachTree(loaded);
                }else{
                    treeRenderer.sprite = treeCreator.GetRandomBeachTree();
                }
                break;
            case "desert":
                if (loaded!=null)
                {
                    treeRenderer.sprite = treeCreator.GetDesertTree(loaded);
                }else{
                    treeRenderer.sprite = treeCreator.GetRandomDesertTree();
                }
                break;
        }

        return treeRenderer.sprite;
    }

    /// <summary>
    /// Determines size of tree collider. Depending on sprite prefix name, bounding box
    /// is calculated differently.
    /// </summary>
    /// <param name="collider2D">colldier object</param>
    /// <param name="boundingBox">bounding box around sprite</param>
    private void AdjustTreeCollider(CapsuleCollider2D collider2D, Bounds boundingBox, string name)
    {
        int index = name.IndexOf("_");
        string prefix = name.Substring(0, index);
        switch (prefix)
        {
            case "regular":
                collider2D.size = new Vector2(boundingBox.size.x / 2f, collider2D.size.y);
                break;
            case "big":
                collider2D.size = new Vector2(boundingBox.size.x / 1.2f, collider2D.size.y);
                break;
            case "small":
                collider2D.size = new Vector2(boundingBox.size.x / 5f, collider2D.size.y);
                break;
            default:
                collider2D.size = new Vector2(boundingBox.size.x / 2f, collider2D.size.y);
                break;
        }
        return;

    }

    private void Awake()
    {
        var m = GameObject.FindGameObjectWithTag("Map");
        if (m != null){
            mapReference = m.GetComponent<Map>();
            chunkLoader = m.GetComponent<ChunkLoader>();
            gameHandler = m.GetComponent<GameHandler>();
        }
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        treePool = GetComponent<TreePool>();
    }

    private void Update()
    {
        if(!treesLoaded){
            LoadTrees();
        }
    }
}

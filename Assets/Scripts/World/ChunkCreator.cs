using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Random=UnityEngine.Random;


/// <summary>
/// This Class is handling creation of each chunk. Mesh is created on whose is applied texture representing tiles.
/// This improves performance a big amount.
/// </summary>
public class ChunkCreator : MonoBehaviour
{
    public GameObject treePrefab;
    MeshRenderer meshRenderer;
    MeshFilter meshFilter;

    private float tileSize = 1;
    public Vector3[] vertices;
    public Vector2[] uv;
    public int[] triangles;
    public TDTile[,] tiles;
    //debug values
    public int x;
    public int top_x;
    public int y;
    public int top_y;


    /// <summary>
    /// Function that creates mesh with given width, height and world space. This tile represents
    /// one chunk. Mesh is divided into 32 x 32 quads. Each quad represents 1 world tile. Each tile's
    /// UV is set accordingly to match texture desired. Consists of 2 loops throught the map, first one
    /// is creating quads for tiles and setting it's biomes, while second one sets each tile it's neighbours
    /// (flood filling) and setting textures accordingly.
    /// </summary>
    /// <param name="width">chunk width</param>
    /// <param name="height">chunk height</param>
    /// <param name="chunkX">key x coord(bot left)</param>
    /// <param name="chunkY">key y coord(bot left)</param>
    /// <returns></returns>
    public Mesh CreateTileMesh(int width, int height, int chunkX, int chunkY) {
        x= chunkX;
        y=chunkY;
        top_x = x+32;
        top_y = y+32;
        Mesh mesh = new Mesh();

        this.vertices = new Vector3[4 * (width * height)];
        this.uv = new Vector2[4 * (width * height)];
        this.triangles = new int[6 * (width * height)];

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                int index = x * height + y;
                vertices[index * 4 + 0] = new Vector3(tileSize * x + chunkX,         tileSize * y + chunkY);
                vertices[index * 4 + 1] = new Vector3(tileSize * x + chunkX,         tileSize * (y + 1) + chunkY);
                vertices[index * 4 + 2] = new Vector3(tileSize * (x + 1) + chunkX,   tileSize * (y + 1) + chunkY);
                vertices[index * 4 + 3] = new Vector3(tileSize * (x + 1) + chunkX,   tileSize * y +chunkY);
                
                triangles[index * 6 + 0] = index * 4 + 0;
                triangles[index * 6 + 1] = index * 4 + 1;
                triangles[index * 6 + 2] = index * 4 + 2;

                triangles[index * 6 + 3] = index * 4 + 0;
                triangles[index * 6 + 4] = index * 4 + 2;
                triangles[index * 6 + 5] = index * 4 + 3;
  
                /* set biome for each tile, and pointers to 4 direction neighbours*/
                SetTileBiome(x,y,chunkX,chunkY);
                SpawnTree(x,y,chunkX, chunkY);
            }
        }

        /* now set UVs and textures accordingly */
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                int index = x * height + y;
                Sprite tileSprite = GetTileTexture(x,y,chunkX,chunkY);

                //map UVs for each tile to specific texture in atlas
                this.uv[index *4+0] = SetTileTexture(0,tileSprite);
                this.uv[index *4+1] = SetTileTexture(1,tileSprite);
                this.uv[index *4+2] = SetTileTexture(2,tileSprite);
                this.uv[index *4+3] = SetTileTexture(3,tileSprite); 
            }
        }
        
        
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;

        

        GetComponent<MeshFilter>().mesh = mesh;
        return mesh;
    }

    /// <summary>
    /// Spawn trees in loaded chunk
    /// </summary>
    /// <param name="x">x coordinate</param>
    /// <param name="y">y coordinate</param>
    /// <param name="chunkX">Chunk x key</param>
    /// <param name="chunkY">Chunk y key</param>
    private void SpawnTree(int x, int y, int chunkX, int chunkY){
        var mapReference = transform.parent.gameObject.GetComponent<Map>();
        int2 chunkKey = new int2(chunkX, chunkY);
        int2 relativePos = new int2(x,y);
        
        TDTile tile = mapReference.GetTile(relativePos, chunkKey);
        WorldChunk chunk = mapReference.GetChunk(chunkKey);

        if (chunk.treeMap[x,y] == 1 
        && tile.biome.type != "ocean"
        && tile.biome.type != "beach"
        && tile.biome.type != "water"
        && tile.hillEdge == EdgeType.none 
        && tile.edgeType == EdgeType.none)
        {
            GameObject tree = Instantiate(treePrefab, new Vector3(0,0,0), Quaternion.identity);
            tree.transform.parent = gameObject.transform;
            tree.transform.position = new Vector3(chunkX+x, chunkY+y, 0);
        }
    }

    /// <summary>
    /// Sets tile's neighbourhood(pointers in 8 directions) and sets it's biome accordingly.
    /// </summary>
    /// <param name="x">x coord</param>
    /// <param name="y">y coord</param>
    /// <param name="chunkX">chunk x key</param>
    /// <param name="chunkY">chunk y key</param>
    private void SetTileBiome(int x, int y, int chunkX, int chunkY){
        //bool isLeftSame, isTopLeftSame, isTopSame, isTopRightSame, isRightSame, isBotRightSame, isBotSame, isBotLeftSame, sameAround = false;
        var mapReference = transform.parent.gameObject.GetComponent<Map>();

        mapReference.AssignNeighbours(mapReference.GetTile(new int2(x,y), new int2(chunkX, chunkY)), new int2(chunkX, chunkY));
    }

    /// <summary>
    /// Return texture according to given coordinatite
    /// </summary>
    /// <param name="x">x coord</param>
    /// <param name="y">y coord</param>
    /// <param name="chunkX">chunk x key</param>
    /// <param name="chunkY">chunk y key</param>
    /// <returns>Texture for tile</returns>
    private Sprite GetTileTexture(int x, int y, int chunkX, int chunkY){
        var mapReference = transform.parent.gameObject.GetComponent<Map>();
        // get reference to tile working with
        TDTile tile = mapReference.GetTile(new int2(x,y), new int2(chunkX, chunkY));
        //set material texture so we can assign uvs later on 
        SetMaterialTexture(tile.biome.GetRandomSprite().texture);
        //problematic tiles
        Sprite ret = tile.biome.GetTileSprite(tile);
        return ret;
    }
    /// <summary>
    /// Sets UV's for quads within meshes for each tile. 
    /// </summary>
    /// <param name="corner">Corner of quad that is being processed.</param>
    /// <param name="tileSprite">Sprite holding texture</param>
    /// <returns>Vector2 UV for given corner of quad.</returns>
    private Vector2 SetTileTexture(int corner, Sprite tileSprite){
        Rect UVs = tileSprite.rect;
        UVs.x /= tileSprite.texture.width;
        UVs.width /= tileSprite.texture.width;
        UVs.y /= tileSprite.texture.height;
        UVs.height /= tileSprite.texture.height;
        /*
            Return UV for each corner of texture within atlas
        */
        Vector2 retval = new Vector2(0,0);
        switch (corner)
        {
            case 0: //bottom left
                retval = new Vector2(UVs.x,UVs.y);
                break;
            case 1: //top left
                retval = new Vector2(UVs.x,UVs.y + UVs.height);
                break;
            case 2: //top right
                retval = new Vector2(UVs.x + UVs.width,UVs.y + UVs.height);
                break;
            case 3: //bottom right
                retval = new Vector2(UVs.x + UVs.width,UVs.y);
                break;
        }
        
        return retval;
    }
    /// <summary>
    /// Set material texture holding all tile textures to be used in meshes.
    /// </summary>
    /// <param name="texture">Tiles texture</param>
    private void SetMaterialTexture(Texture2D texture){
        GetComponent<MeshRenderer>().sharedMaterials[0].mainTexture = texture;
        return;
    }

}

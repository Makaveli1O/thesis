using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
public class ChunkLoader : MonoBehaviour
{/*
    int chunkSize = 32;
    public int width, height;
    public float scale = 1.0f; 
    public int seed;
    //height
    public int heightSeed;
    public int heightOctaves;
    public float heightFrequency;
    public float heightExp;
    //precipitation
    public int  precipitationSeed;
    public int precipitationOctaves;
    public float precipitationPersistance;
    public float precipitationLacunarity;
    //heat
    public float temperatureMultiplier; //increasing this will make poles move more further to equator ->default 1.0f
    public float temperatureLoss; //temperature loss for each height increase

    ChunkGenerator chunkGenerator = null;
    private int maxLoadedChunks = 9;
    private List<WorldChunk> loadedChunks;
    private Dictionary<int2, WorldChunk> chunkStorage = new Dictionary<int2, WorldChunk>();

    public GameObject chunkPrefab;

    void Start(){
        this.chunkGenerator = GetComponent<ChunkGenerator>();
    }

    public void Initialize( int width, int height, float scale, int seed,
                        int heightSeed, int heightOctaves, float heightFrequency, float heightExp,
                        int precipitationSeed, int precipitationOctaves, float precipitationPersistance, float precipitationLacunarity,
                        float temperatureMultiplier, float temperatureLoss, Dictionary<int2, WorldChunk> perlinChunks){

        this.loadedChunks = new List<WorldChunk>(maxLoadedChunks);
        this.width = width;
        this.height = height;
        this.scale = scale;
        this.seed = seed;

        this.heightSeed = heightSeed;
        this.heightOctaves = heightOctaves;
        this.heightFrequency = heightFrequency;
        this.heightExp = heightExp;

        this.precipitationSeed = precipitationSeed;
        this.precipitationOctaves = precipitationOctaves;
        this.precipitationPersistance = precipitationPersistance;
        this.precipitationLacunarity = precipitationLacunarity;

        this.temperatureLoss = temperatureLoss;
        this.temperatureMultiplier = temperatureMultiplier;

        this.chunkStorage = perlinChunks;
    }

    public void LoadChunk(Vector3 PlayerPos, int2 chunkKey){
        this.loadedChunks.Add(this.GenerateChunk(chunkKey));
        Debug.Log(loadedChunks.Count);
        foreach ( var chunk in loadedChunks )
        {
            //create chunk object
            GameObject chunkP = Instantiate(chunkPrefab, new Vector3(0,0,0), Quaternion.identity);
            chunkP.transform.parent = gameObject.transform;
            ChunkCreator chunkCreator = chunkP.GetComponent<ChunkCreator>(); //reference to script
            //create mesh (chunk) and save it to structure holding chunk
            chunk.chunkMesh = chunkCreator.CreateTileMesh(chunkSize,chunkSize, chunkKey.x, chunkKey.y);
        }
    }

    public void UnloadChunk(WorldChunk chunk){
        
    }

    public WorldChunk GenerateChunk(int2 chunkKey){
        WorldChunk newChunk = null;
        newChunk = chunkGenerator.GenerateLandMass(newChunk, heightOctaves, heightFrequency, heightExp, scale, seed, heightSeed, new int2(width,height));
        newChunk = chunkGenerator.GenerateHeatMap(newChunk, new int2(width,height), temperatureMultiplier, temperatureLoss);
        newChunk = chunkGenerator.GeneratePrecipitationMap(newChunk, seed, precipitationSeed, scale, precipitationOctaves, precipitationPersistance, precipitationLacunarity, new int2(width,height));
        
        return newChunk;
    }

    public void UnloadChunk(){

    }*/
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics; //int2 and perlin noise
public class ChunkGenerator : MonoBehaviour
{
    private int chunkSize = 32;
    /*
        This function requires x,y coords and chunks dictionary. If chunk is already present within
        dictionary, same dictionary is returned. If it's not, new EMPTY chunk will be added to the dictionary.

        Returns appended (or same) dictionary..

        lowering persistance -> bigger clusters(less detail)
    */
    public Dictionary<int2, WorldChunk> GenerateChunk(int x, int y, Dictionary<int2, WorldChunk> chunks, int globalSeed, int seed, float scale, int octaves, float persistance, float lacunarity, string chunksType, int2 dimensions, Dictionary<int2, WorldChunk> heightChunks = null){
        var key = new int2(x,y);
        //chunk is not yet loaded
        if (!chunks.ContainsKey(key))
        {
            //append dictionary
            chunks.Add(key, new WorldChunk(chunkSize));
            chunks[key].position = new int2(x,y);
        }
        switch (chunksType)
        {
            case "height":
                chunks[key] = GenerateLandMass(chunks[key], octaves, persistance,lacunarity, scale,globalSeed, seed, dimensions);
                break;
            case "precipitation":
                chunks[key] = GeneratePrecipitationMap(chunks[key],globalSeed,seed, scale, octaves, persistance, lacunarity, dimensions);
                break;
            case "temperature":
                //persistance is temperatureMultiplier in this case
                chunks[key] = GenerateHeatMap(chunks[key], dimensions, persistance, lacunarity); 
                break;
        }
        
        return chunks;

    }

    /*
        @function GeneratePrecipitationMap
        Creates moisture map for 1 chunk, depending on given seeds, scale, octaves, persistance and lucanarity.
        More octaves will result in more processed noise.
        @return Chunk of precipitation data.
    */
    public WorldChunk GeneratePrecipitationMap(WorldChunk chunk, int globalSeed, int seed, float scale, int octaves, float persistance, float lacunarity, int2 dimensions, bool temperature = false){
        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                float frequency = 1f; //higher frequency -> further apart sample points height values will change more rapidly
                float amplitude = 1f;
                float noiseHeight = 0f;

                for (int i = 0; i<octaves; i++){
                    float px = (float)(x + chunk.position.x) / chunkSize * scale * frequency;//* 1f + xOffset;
                    float py = (float)(y + chunk.position.y) / chunkSize * scale * frequency;//*1f + yOffset;
                    float perlinValue = Mathf.PerlinNoise(px + globalSeed + seed,py + globalSeed + seed);
                    noiseHeight += perlinValue * amplitude; 
                    amplitude *= persistance; //decreases
                    frequency *= lacunarity; //increases
                }
                chunk.sample[x,y].precipitation = noiseHeight;
            }
        }
        return chunk;
    }

    /*
        This function generates landmass(island like structure) with perlin noise. Only generate chunk by chunk

        @chunk -> current generated chunk (32x32)
        @octaves -> number of octaves for perlin noise to merge
        @frequency -> frequency of the noise (higher frequency more distanced the values)
        @exp -> exponential of the noise (higher means less land and more water)
        @scale -> scale of the world generated
        @globalSeed -> global seed of each map
        @seed -> seed only for height map

        Returns altered chunk within chunks dictionary.
    */
    public WorldChunk GenerateLandMass(WorldChunk chunk, int octaves, float frequency, float exp, float scale, int globalSeed, int seed, int2 dimensions){
        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                //create new tdtile instance
                chunk.sample[x,y] = new TDTile(); //create new tile instance
                float elevation = 0f;
                float amplitude = 1f;
                float f = frequency;
                float amplitudeSum = 0f;
                for (int i = 0; i<octaves; i++){
                    float px = (float)(x + chunk.position.x) / chunkSize * scale;
                    float py = (float)(y + chunk.position.y) / chunkSize * scale;
                    elevation = Mathf.PerlinNoise(px * f+seed+globalSeed,py * f+seed+globalSeed); 

                    //run perlin values through function tat will make square island like shape

                    elevation = MakeIslandMask(dimensions.x, dimensions.y,x+chunk.position.x, y + chunk.position.y, elevation,chunk.sample[x,y]);
                    f = Mathf.Pow(f, 2);
                    if (f == 1f)
                    {
                        f = 2f;
                    }
                    amplitudeSum += amplitude;
                    amplitude /= 2;
                }

                elevation = elevation / amplitudeSum;
                //redistribution and initialization of tile
                chunk.sample[x,y].pos.x = chunk.position.x + x;
                chunk.sample[x,y].pos.y = chunk.position.y + y;
                chunk.sample[x,y].height = Mathf.Pow(elevation,exp);
                chunk.sample[x,y].z_index = SetZ_Index(elevation);

                // numbers like -0,00115059^1.48 returns nan dunno why NaN
                if (float.IsNaN(chunk.sample[x,y].height))
                {
                   chunk.sample[x,y].height = Mathf.Pow(Mathf.Abs(elevation),exp);
                }
            }
        }
        
        return chunk;
    }

    private int SetZ_Index(float elevation){
        if (elevation > 0.25 && elevation < 0.7){        //1st level
            return 1;
        }else if (elevation > 0.7 && elevation < 1){  //2nd level
            return 2;
        }else {
            return 0;                                   //water level 0th level
        }
    }

    /*
        @function GenerateHeatMap
        Creates part of heat map with elevation/latitude relationship. The higher latitude(distance from equator) is
        the lower temperature will be. Elevation also decreases temperature of area, how much tho depends on temperature_multiplier argument.
        (Gradient - cold on poles and hot on equator)
        @return Chunk of heat map.
    */
    public WorldChunk GenerateHeatMap(WorldChunk chunk, int2 dimensions, float temperature_multiplier, float temperature_loss){
        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                float elevation = chunk.sample[x,y].height;

                int latitude = GetLatitude(chunk.position.y + y, dimensions);
                float heatValue = GenerateHeatValue(latitude, dimensions.y, elevation,temperature_multiplier, temperature_loss);
                chunk.sample[x,y].temperature = heatValue;
            }
        }
        return chunk;
    }
    
     /*
        @function GenerateHeatValue
        @param latitude -> distance from equator
        @param temperature_multiplier   -> multiplier decreasing the temperature value with increasing latitude
        @param temperature_loss         -> is the amount of temperature that should be lost for each height step.
        @param base_temperature         -> is the highest temperature at the equator

        Formula for calculating heat of given tile:
        heat = ((latitude / (map_height)) *-temperature_multiplier) - ((elevation / temperature_height) * temperature_loss) + base_temperature;
        or
         //float retval = ((latitude / map_height) * temperature_multiplier) - ((elevation / temperature_height) * temperature_loss); 
        @return -> float heat value for given tile.
    */
    private static float GenerateHeatValue(int latitude, int map_height, float elevation, float temperature_multiplier,float temperature_loss){
        float tmp = (float) (latitude / (map_height/2f) * temperature_multiplier - (elevation * temperature_loss));
        float retval = 1f - tmp;
        retval = Mathf.Clamp(retval, 0.0f, 1.0f);

        return retval;
    }
    /*
        @function GetLatitude
        Takes two arguments, y coord and dimensions of map. Returns latitude of given y coord.
    */
    private static int GetLatitude(int y, int2 dimensions){
        int equator = dimensions.y / 2;
        int latitude = EuclidDistance(y, equator);
        return latitude;
    }
    /*
        @function EuclidDistance
        Returns distance between two points(on same axis).
    */
    private static int EuclidDistance(int y1, int y2){
        return Mathf.Abs(y1-y2);
    }

    /*
        ISLAND(heightmap) STUFF
    */
     public static float MakeIslandMask(int width, int height, int posX, int posY, float oldValue, TDTile tile) {
        int minVal =(((height + width )/2)/ 100 * 2 );
        int maxVal = (((height + width)/2)/ 100 * 10 );
        if(GetDistanceToEdge(posX, posY, width, height) <= minVal){
            tile.landmass = false;
            return 0;
        }else if(GetDistanceToEdge(posX, posY, width, height) >= maxVal){
            tile.landmass = true;
            return oldValue;
        }else{
            float factor = GetFactor(GetDistanceToEdge(posX, posY, width, height), minVal, maxVal);
            //0.1 on 512x512 , 0.15 on 256x256
            float treshold = 0.1f;
            if((width >= 512 && height >= 512)){
                treshold = 0.1f;
            }else if((width >= 256 && height >= 256)){
                treshold = 0.15f;
            }
            if (factor * oldValue < treshold)
            {
                tile.landmass = false;
            }else{
                tile.landmass = true;
            }
            return oldValue * factor;
        }
    }
 
    /*
        @function GetFactor
        Finds out factor of given value and returns it.
        Used in creating island-like maps.
    */
    private static float GetFactor( int val, int min, int max ) {
        int full = max - min;
        int part = val - min;
        float factor = (float)part / (float)full;
        return factor;
    }
    /*
        @function GetDistanceToEdge
        Finds minimal distance to the edge and returns it.
        Used in creating island-like  maps.
    */
    private static int GetDistanceToEdge(int x, int y, int width, int height) {
        int[] distances = new int[]{y, x,(width - x), (height - y)};
        int min = distances[ 0 ];
        foreach(var val in distances) {
            if(val < min) {
                min = val;
            }
        }
        return min;
    }
}
/*
Helping class holding once chunk of map data.
*/
public class WorldChunk
{
    public int2 position; //start corner of chunk
    public TDTile[,] sample;
    public Mesh chunkMesh;
    public WorldChunk(int chunkSize = 32){
        sample = new TDTile[chunkSize, chunkSize];
    }
}

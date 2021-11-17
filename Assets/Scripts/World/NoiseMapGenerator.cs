using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    This Class handles map creation(procedural generation).
    BAsically heat(or height) map generation.
*/
public class NoiseMapGenerator : MonoBehaviour
{

    /*
        Function GenerateNoiseMap
        @param mapHeight -> height of the map
        @param mapWidth -> width of the map
        @param seed -> seeding each map so we wont sample the same noise thus this wont result in same result with different seed
        @param scale -> "zoom" factor of generating noise.
        @param offeset -> offseting the map (as we move on the map we want to generate it further)
        @param playerPos -> position of player

        @Return float 2d array map.
    */
    public float [,] GenerateNoiseMap(int mapWidth,int mapHeight, float seed,float scale){
        float [,] noiseMap = new float[mapWidth,mapHeight];
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                //samples
                float samplePosX = (float) x * scale;
                float samplePosY = (float) y * scale;
                noiseMap[x,y] = Mathf.PerlinNoise(samplePosX /mapWidth + seed , samplePosY /mapHeight +seed);
            }
        }
        return noiseMap;
    }

}
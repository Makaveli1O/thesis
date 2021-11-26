using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeCreator : MonoBehaviour
{
    public List<BiomePreset> biomes;

    private Sprite[] forestTrees;
    private Sprite[] ashlandTrees;
    private Sprite[] jungleTrees;
    private Sprite[] beachTrees;
    private Sprite[] desertTrees;

    public Sprite GetRandomForestTree(){
        return forestTrees[Random.Range(0, forestTrees.Length)];
    }

    public Sprite GetRandomAshlandTree(){
        return ashlandTrees[Random.Range(0, ashlandTrees.Length)];
    }

    public Sprite GetRandomJungleTree(){
        return jungleTrees[Random.Range(0, jungleTrees.Length)];
    }

    public Sprite GetRandomBeachTree(){
        return beachTrees[Random.Range(0, beachTrees.Length)];
    }

    public Sprite GetRandomDesertTree(){
        return desertTrees[Random.Range(0, desertTrees.Length)];
    }

    void Awake(){
        foreach (BiomePreset biome in this.biomes)
        {
           foreach (TreeCategory treeCat in biome.trees)
           {
                if (treeCat.name == "basic")
                {
                    switch(biome.type){
                        case "forest":
                            forestTrees = treeCat.sprites;
                            break;
                        case "rainforest":
                            jungleTrees = treeCat.sprites;
                            break;
                        case "beach":
                            beachTrees = treeCat.sprites;
                            break;
                        case "desert":
                            desertTrees = treeCat.sprites;
                            break;
                        case "ashland":
                            ashlandTrees = treeCat.sprites;
                            break;
                    }
                }
           }
        }
    }

}

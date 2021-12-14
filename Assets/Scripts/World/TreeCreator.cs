using System.Collections.Generic;
using UnityEngine;

public class TreeCreator : MonoBehaviour
{
    public List<BiomePreset> biomes;
    [SerializeField] public int x;
    [SerializeField] public int y;
    private Sprite[] forestTrees;
    private Sprite[] ashlandTrees;
    private Sprite[] jungleTrees;
    private Sprite[] beachTrees;
    private Sprite[] desertTrees;

    public Sprite GetRandomForestTree(){
        return forestTrees[Random.Range(0, forestTrees.Length)];
    }

    public Sprite GetForestTree(string name){
        foreach (Sprite sprite in forestTrees)
        {
            if (sprite.name == name)
            {
                return sprite;
            }
        }
        return null;
    }

    public Sprite GetRandomAshlandTree(){
        return ashlandTrees[Random.Range(0, ashlandTrees.Length)];
    }

    public Sprite GetAshlandTree(string name){
        foreach (Sprite sprite in ashlandTrees)
        {
            if (sprite.name == name)
            {
                return sprite;
            }
        }
        return null;
    }

    public Sprite GetRandomJungleTree(){
        return jungleTrees[Random.Range(0, jungleTrees.Length)];
    }

    public Sprite GetJungleTree(string name){
        foreach (Sprite sprite in jungleTrees)
        {
            if (sprite.name == name)
            {
                return sprite;
            }
        }
        return null;
    }

    public Sprite GetRandomBeachTree(){
        return beachTrees[Random.Range(0, beachTrees.Length)];
    }

    public Sprite GetBeachTree(string name){
        foreach (Sprite sprite in beachTrees)
        {
            if (sprite.name == name)
            {
                return sprite;
            }
        }
        return null;
    }

    public Sprite GetRandomDesertTree(){
        return desertTrees[Random.Range(0, desertTrees.Length)];
    }

    public Sprite GetDesertTree(string name){
        foreach (Sprite sprite in desertTrees)
        {
            if (sprite.name == name)
            {
                return sprite;
            }
        }
        return null;
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeCreator : MonoBehaviour
{
    //z_index 1 forest trees
    [SerializeField] private Sprite [] forestTrees;


    public Sprite GetForestTree(){
 
            return forestTrees[Random.Range(0, forestTrees.Length)];
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
/*
    this calss is used for inspector handling map dimensions in real time without
    initializing and starting the game
*/
[CustomEditor (typeof (Map))]
public class MapGeneratorEditor : Editor
{
    public override void OnInspectorGUI(){
        Map mapGen = (Map)target;
        //if any value was changed
        if (DrawDefaultInspector()){
            if (mapGen.autoUpdate)
            {
                mapGen.DestroyChildPrefabs();
                mapGen.MapGeneration();
            }
        }

        if (GUILayout.Button("Generate"))
        {
            mapGen.DestroyChildPrefabs();
            mapGen.MapGeneration();
        }
        if (GUILayout.Button("Destroy"))
        {
            mapGen.DestroyChildPrefabs();
        }
    }
}

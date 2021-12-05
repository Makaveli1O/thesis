using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

/// <summary>
/// Gamestate handling class. This class handles retrieving correct gamestate. Correctly
/// retrieve map and player's states. Such as positions, numbers textures etc.
/// </summary>
public class GameHandler : MonoBehaviour
{
    float world_seed; //parent folder for all things within this seed
    private void Awake(){
        Map mapRef = GetComponent<Map>();
        world_seed = mapRef.seed;
        SaveSystem.Init();
    }

    public void Save(ObjType type, Vector3 position){
        Debug.Log("Saving game state.");
        //create save object
        SaveObject saveObj = new SaveObject{
            type = type,
            objPos  = position
        };

        string json = JsonUtility.ToJson(saveObj);

        SaveSystem.Save(json,world_seed+"_player.json");
    }

    public SaveObject Load(){
        Debug.Log("Loading game state.");
        //read saved json
        string saveString = SaveSystem.Load(world_seed+"_player.json");
        SaveObject saveObj = null;
        if (saveString != null)
        {
            //json serialize
            saveObj = JsonUtility.FromJson<SaveObject>(saveString);
        }
        return saveObj;
    }
}

public class SaveObject
{
    public ObjType type;
    public Vector3 objPos;
}

public enum ObjType{
    Player,
    Entity,
    Chunk,
}

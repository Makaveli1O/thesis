using UnityEngine;
using System.IO;
using Unity.Mathematics;
using System.Runtime.Serialization.Formatters.Binary;

using System.Collections.Generic;

/// <summary>
/// Gamestate handling class. This class handles retrieving correct gamestate. Correctly
/// retrieve map and player's states. Such as positions, numbers textures etc.
/// </summary>
public class GameHandler : MonoBehaviour
{
    private string SAVE_FOLDER;
    float world_seed; //parent folder for all things within this seed
    private void Awake(){
        Map mapRef = GetComponent<Map>();
        world_seed = mapRef.seed;
        SaveSystem.Init();
        SAVE_FOLDER = Application.dataPath + "/Saves/";
    }

     public void Save<T>(T saveObj, ObjType key, Vector3 position){
        string json = JsonUtility.ToJson(saveObj);

        if(key == ObjType.Player){
        SaveSystem.Save(json,world_seed+"_"+key+".json");
        //save chunks with key in name
        }else{
            SaveSystem.Save(json,world_seed+"_"+key+"_"+position.x+","+position.y+".json");
        }
    }

    public T Load<T>(ObjType key, int x_pos = -1, int y_pos = -1){
        //read saved json
        string saveString = null;

        //load on specific coords
        if (x_pos != -1 || y_pos != -1){
            saveString = SaveSystem.Load(world_seed+"_"+key+"_"+x_pos+","+y_pos+".json");
        }else{
            saveString = SaveSystem.Load(world_seed+"_"+key+".json");
        }
        
        T returnValue = default(T);
        if (saveString != null)
        {
            //json serialize
            returnValue = (T)JsonUtility.FromJson<T>(saveString);
        }

        return returnValue;
    }
}

[System.Serializable]
public class SavePlayer
{
    public Vector3 pos;
    public SavePlayer(Vector3 pos){
        this.pos = pos;
    }
}
[System.Serializable]
public class SaveChunk
{
    public Vector3 pos;

    public SaveChunk(Vector3 pos){
        this.pos = pos;
    }
}

public enum ObjType{
    Player,
    Entity,
    Chunk,
}


    
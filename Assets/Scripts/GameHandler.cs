using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;

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
    //TODO doc
     public void Save<T>(T saveObj, ObjType key, Vector3 position){
        string json = JsonUtility.ToJson(saveObj);

        if(key == ObjType.Player){
            SaveSystem.Save(json,key+".json", world_seed.ToString());
            //saving key objects
        }else if(key == ObjType.KeyObjects){
            SaveSystem.Save(json,key+".json", world_seed.ToString());  
        //save chunks with key in name
        }else{
            SaveSystem.Save(json,key+"_"+position.x+","+position.y+".json", world_seed.ToString());
        }
    }
//TODO doc
    public T Load<T>(ObjType key, int x_pos = -1, int y_pos = -1){
        //read saved json
        string saveString = null;

        //load on specific coords
        if (x_pos != -1 || y_pos != -1){
            saveString = SaveSystem.Load(key+"_"+x_pos+","+y_pos+".json", world_seed.ToString());
        }else{
            saveString = SaveSystem.Load(key+".json", world_seed.ToString());
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
//TODO doc
[System.Serializable]
public class SavePosition
{
    public Vector3 pos;
    public SavePosition(Vector3 pos){
        this.pos = pos;
    }
}
//TODO comment
[System.Serializable]
public class SaveKeyObjects
{
    public List<Vector3> positions;

    public SaveKeyObjects(List<TDTile> tiles){
        this.positions = new List<Vector3>();
        foreach (TDTile t in tiles)
        {
            int2 coord = new int2(t.pos.x, t.pos.y);
            Vector3 pos = new Vector3(coord.x, coord.y, 0);
            this.positions.Add(pos);
        }
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
    KeyObjects,
}


    
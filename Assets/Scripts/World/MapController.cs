using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;


/// <summary>
/// Class that handles interaction between player and generated world.
/// Checks player's position every frame, and determines whenever player
/// can move to certain tile or not.
/// </summary>
public class MapController : MonoBehaviour
{
    Vector3 playerPos;
    Map mapObj;
    private GameObject playerObj = null;
    private GameHandler gameHandler = null;
    void Start()
    {
        mapObj = GetComponent<Map>();   //get reference to map
        gameHandler = GetComponent<GameHandler>();

        if (playerObj == null)          //get player obj
            playerObj = GameObject.Find("Player");

        //beggining position ( spawn )
        SaveObject playerSave = gameHandler.Load();
        playerPos = playerSave.objPos;
    }

    void Update()
    {
        int xPos = (int)playerObj.transform.position.x;
        int yPos = (int)playerObj.transform.position.y;
        int zPos = (int)playerObj.transform.position.z;
        

        int2 chunkKey = mapObj.TileChunkPos(new int2(xPos, yPos));
        int2 relativePos = mapObj.TileRelativePos(new int2(xPos, yPos));

        TDTile tile = mapObj.GetTile(relativePos, chunkKey);

        if (tile.IsWalkable)
        {   
            playerPos = new Vector3(playerObj.transform.position.x,playerObj.transform.position.y,0);
        }else{
            playerObj.transform.position = playerPos;
        }
    }
    void OnApplicationQuit() {
        gameHandler.Save(ObjType.Player,playerPos);
    }
}

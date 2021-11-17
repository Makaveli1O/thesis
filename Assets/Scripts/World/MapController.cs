using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

/*  *   *   *   *   *   *   *   *   *   *
Class that handles interaction between player and generated world.
Checks player's position every frame, and determines whenever player
can move to certain tile or not.
*   *   *   *   *   *   *   *   *   *   */
public class MapController : MonoBehaviour
{
    Vector3 playerPos;
    Map mapObj;
    private GameObject playerObj = null;
    void Start()
    {
        mapObj = GetComponent<Map>();   //get reference to map
        if (playerObj == null)          //get player obj
            playerObj = GameObject.Find("Player");

        //beggining position ( spawn )
        playerPos = new Vector3(38,50,0);
    }

    // Update is called once per frame
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
}

using UnityEngine;
using Unity.Mathematics;


/// <summary>
/// Class that handles interaction between player and generated world.
/// Checks player's position every frame, and determines whenever player
/// can move to certain tile or not.
/// </summary>
public class MapController : MonoBehaviour
{
    Vector3 lastPos;
    Vector3 playerPos;
    Map mapObj;
    private GameObject playerObj = null;
    private GameHandler gameHandler = null;
    private PlayerController playerController;
    void Awake()
    {
        mapObj = GetComponent<Map>();   //get reference to map
        gameHandler = GetComponent<GameHandler>();

        if (playerObj == null)          //get player obj
            playerObj = GameObject.Find("Player");

        //beggining position ( spawn )
        try{
            playerController = playerObj.GetComponent<PlayerController>();
            SavePosition playerSave = gameHandler.Load<SavePosition>(ObjType.Player);
            playerPos = playerSave.pos;
            playerObj.transform.position = playerPos;
        }catch{
            //first encounter
            playerPos = new Vector3(50,40,0);
        }

    }

    void Update()
    {
        int xPos = (int)playerObj.transform.position.x;
        int yPos = (int)playerObj.transform.position.y;
        int zPos = (int)playerObj.transform.position.z;

        int2 chunkKey = mapObj.TileChunkPos(new int2(xPos, yPos));
        int2 relativePos = mapObj.TileRelativePos(new int2(xPos, yPos));

        TDTile tile = mapObj.GetTile(relativePos, chunkKey);

        if (tile.IsWalkable) {
            playerPos = new Vector3(playerObj.transform.position.x,playerObj.transform.position.y,0);
        //non-walkable tiles
        }else{
            if(tile.partial){
                TileEdgesMovement(tile,playerPos);
            }else{  //full cliffs unable to walk every where
                playerObj.transform.position = playerPos; // stop
            }
        }
    }
    void OnApplicationQuit() {
        SavePosition savePlayer = new SavePosition(playerPos);
        gameHandler.Save<SavePosition>(savePlayer, ObjType.Player,playerPos);
    }
    //TODO doc
    // movedir.x < 1 = move right
    // movedir.x > 1 = move left
    // movedir.y > 1 = move up
    // movedir.y < 1 = move down
    private void TileEdgesMovement(TDTile tile, Vector3 playerPos){
        float offset = 0.5f;
        EdgeType type = (tile.hillEdge != EdgeType.none) ? tile.hillEdge : tile.edgeType;
        //right Tile
        if (type == EdgeType.right || type == EdgeType.cliffEndRight){
            RightEdgeCollision(tile, type, offset, playerController.moveDir, playerPos);
        }else if (type == EdgeType.left || type == EdgeType.cliffEndLeft){
            LeftEdgeCollision(tile, type, offset, playerController.moveDir, playerPos);
        }else if (type == EdgeType.top){
            TopEdgeCollision(tile, type, offset, playerController.moveDir, playerPos);
        }else if (type == EdgeType.bot || type == EdgeType.cliffEndBot){
            BotEdgeCollision(tile, type, offset, playerController.moveDir, playerPos);
        }else if (type == EdgeType.topRight){
            TopRightEdgeCollision(tile, type, offset, playerController.moveDir, playerPos);
        }else if (type == EdgeType.topLeft){
            //TopLeftEdgeCollision(tile, type, offset, playerController.moveDir, playerPos);
        }
    }

    //FIXME fix to one function
    //TODO documentation, and move to CharacterMovementcontroller
    void RightEdgeCollision(TDTile tile, EdgeType type, float offset, Vector3 dir, Vector3 playerPos){
        if (dir.x > 0)  //moving right
        {
            if (playerObj.transform.position.x < tile.pos.x + offset) //within offset
            {
                this.lastPos = playerObj.transform.position; //mark last position before offset
            }else{  //stop
                playerObj.transform.position = lastPos; // stop
            }
        }else{  //moving left
            if (playerPos.x > tile.pos.x +1)   //coming from the right side ( instant stop)
            {
                lastPos = playerPos;
                playerObj.transform.position = playerPos;
            }
        }
    }

    void LeftEdgeCollision(TDTile tile, EdgeType type, float offset, Vector3 dir, Vector3 playerPos){
        if (dir.x > 0)  //moving right
        {
            if (playerPos.x < tile.pos.x)   //coming from the right side ( instant stop)
            {
                lastPos = playerPos;
                playerObj.transform.position = playerPos;
            }
        }else{  //moving left
            if (playerObj.transform.position.x > tile.pos.x + offset) //within offset
            {
                this.lastPos = playerObj.transform.position; //mark last position before offset
            }else{  //stop
                playerObj.transform.position = lastPos; // stop
            }
        }
    }

    void TopEdgeCollision(TDTile tile, EdgeType type, float offset, Vector3 dir, Vector3 playerPos){
        if (dir.y > 0)  //moving up
        {
            if (playerObj.transform.position.y < tile.pos.y + offset) //within offset
            {
                this.lastPos = playerObj.transform.position; //mark last position before offset
            }else{  //stop
                playerObj.transform.position = lastPos; // stop
            }
        }else{  //moving down
            if (playerPos.y > tile.pos.y +1)   //coming from the top side ( instant stop)
            {
                lastPos = playerPos;
                playerObj.transform.position = playerPos;
            }
        }
    }

    void BotEdgeCollision(TDTile tile, EdgeType type, float offset, Vector3 dir, Vector3 playerPos){
        if (dir.y > 0)  //moving up
        {
            if (playerPos.y < tile.pos.y)   //coming from the bottom side ( instant stop)
            {
                lastPos = playerPos;
                playerObj.transform.position = playerPos;
            }
        }else{  //moving down
            if (playerObj.transform.position.y > tile.pos.y + offset) //within offset
            {
                this.lastPos = playerObj.transform.position; //mark last position before offset
            }else{  //stop
                playerObj.transform.position = lastPos; // stop
            }
        }
    }

    void TopRightEdgeCollision(TDTile tile, EdgeType type, float offset, Vector3 dir, Vector3 playerPos){
        if (dir.x != 0 && dir.y != 0)//diagonal
        {
            offset = 0.3f;
        }
        if (playerPos.x > tile.pos.x || playerPos.y > tile.pos.y)   //outside
        {
            lastPos = playerPos;
            playerObj.transform.position = playerPos;
        }else{  //inside
            if (dir.y > 0)  //moving up
            {
                if (playerObj.transform.position.y < tile.pos.y + offset) //within offset
                {
                    this.lastPos = playerObj.transform.position; //mark last position before offset
                }else{  //stop
                    playerObj.transform.position = lastPos; // stop
                }
            }else if (dir.x > 0)  //moving right
            {
                if (playerObj.transform.position.x < tile.pos.x + offset) //within offset
                {
                    this.lastPos = playerObj.transform.position; //mark last position before offset
                }else{  //stop
                    playerObj.transform.position = lastPos; // stop
                }
            }
        }
    }

    void TopLeftEdgeCollision(TDTile tile, EdgeType type, float offset, Vector3 dir, Vector3 playerPos){
        if (dir.x != 0 && dir.y != 0)//diagonal
        {
            offset = 0.3f;
        }
        if (playerPos.x < tile.pos.x || playerPos.y > tile.pos.y)   //outside
        {
            lastPos = playerPos;
            playerObj.transform.position = playerPos;
        }else{  //inside
            if (dir.y > 0)  //moving up
            {
                if (playerObj.transform.position.y < tile.pos.y + offset) //within offset
                {
                    this.lastPos = playerObj.transform.position; //mark last position before offset
                }else{  //stop
                    playerObj.transform.position = lastPos; // stop
                }
            }else if (dir.x < 0)  //moving left
            {
                if (playerObj.transform.position.x > tile.pos.x + offset) //within offset
                {
                    this.lastPos = playerObj.transform.position; //mark last position before offset
                }else{  //stop
                    playerObj.transform.position = lastPos; // stop
                }
            }
        }
    }

}

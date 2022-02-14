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
        if (type == EdgeType.right){
            RightEdgeCollision(tile, type, offset, playerController.moveDir, playerPos);
        }else if (type == EdgeType.left){
            LeftEdgeCollision(tile, type, offset, playerController.moveDir, playerPos);
        }else if (type == EdgeType.top){
            TopEdgeCollision(tile, type, offset, playerController.moveDir, playerPos);
        }else if (type == EdgeType.bot || type == EdgeType.cliffEndBot){
            BotEdgeCollision(tile, type, offset, playerController.moveDir, playerPos);
        }else if (type == EdgeType.topRight){
            bool belowDiagonal = isInside(tile.pos, new int2(tile.pos.x+1, tile.pos.y), new int2(tile.pos.x, tile.pos.y+1),playerObj.transform.position);
            bool outsideCond = playerPos.y > tile.pos.y+1 || playerPos.x > tile.pos.x+1;
            //move up, right or upright
            bool moveCond = playerController.moveDir.y > 0 || playerController.moveDir.x > 0;
            CornerEdgesCollision(playerPos, belowDiagonal, outsideCond, moveCond);
        }else if (type == EdgeType.topLeft){
            bool belowDiagonal = isInside(tile.pos, new int2(tile.pos.x+1, tile.pos.y), new int2(tile.pos.x+1, tile.pos.y+1),playerObj.transform.position);
            bool outsideCond = playerPos.y > tile.pos.y+1 || playerPos.x < tile.pos.x;
            //move up, left or upleft
            bool moveCond = playerController.moveDir.y > 0 || playerController.moveDir.x < 0;
            CornerEdgesCollision(playerPos, belowDiagonal, outsideCond, moveCond);
        }else if (type == EdgeType.cliffEndRight){
            bool overDiagonal = isInside(tile.pos, new int2(tile.pos.x, tile.pos.y + 1), new int2(tile.pos.x+1, tile.pos.y+1),playerObj.transform.position);
            bool outsideCond = playerPos.y < tile.pos.y || playerPos.x > tile.pos.x +1;
            //move down, right or downright
            bool moveCond = playerController.moveDir.y < 0 || playerController.moveDir.x > 0;
            CornerEdgesCollision(playerPos, overDiagonal, outsideCond, moveCond);
        }else if (type == EdgeType.cliffEndLeft){
            bool overDiagonal = isInside(new int2(tile.pos.x, tile.pos.y + 1), new int2(tile.pos.x + 1, tile.pos.y + 1), new int2(tile.pos.x+1, tile.pos.y),playerObj.transform.position);
            bool outsideCond = playerPos.y < tile.pos.y || playerPos.x < tile.pos.x;
            //move down, left or downleft
            bool moveCond = playerController.moveDir.y < 0 || playerController.moveDir.x < 0;
            CornerEdgesCollision(playerPos, overDiagonal, outsideCond, moveCond);
        }else if(type == EdgeType.cliffRight || type == EdgeType.cliffLeft){
            lastPos = playerPos;
            playerObj.transform.position = playerPos;
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
    //FIXME little bug when diagonal is connecting with the end of tile, there is a little gap to go through
    /// <summary>
    /// Handles corner edges collision check. For given conditions and tiles, checks corresponding values,
    /// and determins movement of player.
    /// </summary>
    /// <param name="playerPos">Position of player</param>
    /// <param name="diagonalCond">Bool value if player matches diagonal condition</param>
    /// <param name="outside">Condition for if player is outside of tile</param>
    /// <param name="moveCond">Condition for movement</param>
    void CornerEdgesCollision(Vector3 playerPos, bool diagonalCond, bool outside, bool moveCond){
        //outside condition check
        if (outside)   
        {
            playerObj.transform.position = playerPos;
        }else{
            //moving inside tile condition
            if (moveCond){  
                if (!diagonalCond){ //crosses diagonal or tile dimensions
                    playerObj.transform.position = lastPos; // stop
                }else{
                    this.lastPos = playerObj.transform.position; //mark last position before offset
                }
            }
        }
        return;
    }

    /// <summary>
    /// A utility function to calculate area of triangle formed by A(x1, y1) B(x2, y2) and C(x3, y3)
    /// </summary>
    /// <param name="x1">point A.x</param>
    /// <param name="y1">point A.y</param>
    /// <param name="x2">point B.x</param>
    /// <param name="y2">point B.y</param>
    /// <param name="x3">point C.x</param>
    /// <param name="y3">point C.y</param>
    /// <returns>Area formed by three points ( triangle ) </returns>
    private float area(float x1, float y1, float x2, float y2, float x3, float y3)
    {
        return Mathf.Abs((x1 * (y2 - y3) +
                         x2 * (y3 - y1) +
                         x3 * (y1 - y2)) / 2.0f);
    }
 
    /*  */
    /// <summary>
    /// A function to check whether point P(player) liesinside the triangle formed
    ///  by A, B and C
    /// </summary>
    /// <param name="a">Point A(tile pos)</param>
    /// <param name="b">Point B (top end tile pos)</param>
    /// <param name="c">Point C(right most tile pos)</param>
    /// <param name="p">Player position</param>
    /// <returns>True/false if for if player is on given triangle (below diagonal of tile)</returns>
    private bool isInside(int2 a, int2 b, int2 c, Vector3 p)
    {
        /* Calculate area of triangle ABC */
        float A = area(a.x, a.y, b.x, b.y, c.x, c.y);
 
        /* Calculate area of triangle PBC */
        float A1 = area(p.x, p.y, b.x, b.y, c.x, c.y);
 
        /* Calculate area of triangle PAC */
        float A2 = area(a.x, a.y, p.x, p.y, c.x, c.y);
 
        /* Calculate area of triangle PAB */
        float A3 = area(a.x, a.y, b.x, b.y, p.x, p.y);
 
        /* Check if sum of A1, A2 and A3 is same as A */
        return (A == A1 + A2 + A3);
    }

}

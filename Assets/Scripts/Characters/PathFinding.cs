using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class PathFinding : MonoBehaviour
{
    private const int STRAIGHT_COST = 10;
    private const int DIAGONAL_COST = 14;
    TDTile startTile;
    TDTile targetTile;
    Map mapRef;
    TDMap map;
    void Awake(){
        try
        {
            GameObject mapObj = GameObject.Find("_Map");
            mapRef = mapObj.GetComponent<Map>();
            this.map = mapRef.map;
        }
        catch
        {
            Debug.Log("_Map reference not found.");
        }
    }

    //TODO doc
    //FIXME pretypovanie prerobit
    public List<Vector3> FindPathVector(Vector3 start, Vector3 target){
        List<TDTile> path = FindPath(new int2((int)start.x,(int) start.y), new int2((int)target.x, (int)target.y));
        
        //path not found
        if (path == null)
        {
            return null;
        }else{
            List<Vector3> vectorPath = new List<Vector3>();
            foreach (TDTile pathTile in path)
            {
                vectorPath.Add(new Vector3(pathTile.pos.x, pathTile.pos.y, 0));
            }
            return vectorPath;
        }
    }
    //TODO doc
    public List<TDTile> FindPath(int2 startPos, int2 targetPos){
        //get correct tile ref
        this.startTile = mapRef.GetTile(mapRef.TileRelativePos(startPos), mapRef.TileChunkPos(startPos));
        this.targetTile = mapRef.GetTile(mapRef.TileRelativePos(targetPos), mapRef.TileChunkPos(targetPos));
        //invalid path
        if (startTile == null || targetTile == null)
        {
            return null;
        }
        
        //two sets
        List<TDTile> openSet = new List<TDTile>();
        HashSet<TDTile> closedSet = new HashSet<TDTile>();

        openSet.Add(startTile);
        while(openSet.Count > 0){
            TDTile currentTile = openSet[0];

            for (int i = 1; i < openSet.Count; i++)
            {
                //find lowest fCost
                if (openSet[i].fCost < currentTile.fCost || openSet[i].fCost == currentTile.fCost)
                {
                    if (openSet[i].hCost < currentTile.hCost)
                    {
                        currentTile = openSet[i];
                    }
                }
            }
            //remove from openm set and add to closed
            openSet.Remove(currentTile);
            closedSet.Add(currentTile);
            //path found
            if (currentTile == targetTile){
                return RetracePath(startTile, targetTile);
            }
            //check neighbours
            foreach (TDTile neighbour in currentTile.GetNeighbourList())
            {
                //check if is not walkable or in closed
                if (!neighbour.IsWalkable || closedSet.Contains(neighbour)){
                    continue;
                }//if newpath to neighbour is shorter OR neighbour is not in OPEN
                int newCostToNeighbour = currentTile.gCost + GetDistance(currentTile, neighbour);
                if (newCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour)){
                    //set neighbour's fcost
                    neighbour.gCost = newCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, targetTile);
                    //set where you came from to this tile
                    neighbour.cameFrom = currentTile;
                    if (!openSet.Contains(neighbour))
                        openSet.Add(neighbour);
                }
            }
        }

        //no path found
        return null;
    }

    //TODO doc
    private List<TDTile> RetracePath(TDTile startTile, TDTile endTile){
        List<TDTile> path = new List<TDTile>();
        TDTile currentTile = endTile;

        //retrace back to beginning
        while (currentTile != startTile)
        {
            path.Add(currentTile);
            currentTile = currentTile.cameFrom;
        }

        path.Reverse();
        return path;
    }

    //14 y + 10(x - y)
    //TODO doc
    private int GetDistance(TDTile a, TDTile b) {
        int xDistance = Mathf.Abs(a.pos.x - b.pos.x);
        int yDistance = Mathf.Abs(a.pos.y - b.pos.y);
        int remaining = Mathf.Abs(xDistance - yDistance);
        return DIAGONAL_COST * Mathf.Min(xDistance, yDistance) + STRAIGHT_COST * remaining;
    }
}

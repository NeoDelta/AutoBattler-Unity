using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    public List<Tile> tiles = new List<Tile>();

    private void Start()
    {
        if(tiles.Count == 0)
            tiles = new List<Tile>(GetComponentsInChildren<Tile>());

        GameManager.Instance.combatStart.AddListener(DisableTiles);
        GameManager.Instance.preparationStart.AddListener(EnableTiles);
    }

    private void EnableTiles()
    {
        foreach (Tile t in tiles)
            t.gameObject.SetActive(true);
    }

    private void DisableTiles()
    {
        foreach (Tile t in tiles)
            t.gameObject.SetActive(false);
    }

    public void CleanTiles()
    {
        foreach(Tile t in tiles)
        {
            t.SetEmpty();
        }
    }

    public List<Tile> PathFinding(Tile startTile, Tile endTile)
    {
        Dictionary<Tile, Tile> parents = new Dictionary<Tile, Tile>();

        List<Tile> openList = new List<Tile>();
        List<Tile> closeList = new List<Tile>();

        //Initialize costs and parents indices
        Dictionary<Tile, Vector2> costs = new Dictionary<Tile, Vector2>(); //x for g and y for h costs

        foreach (Tile t in tiles)
        {
            float distance = Mathf.Abs((t.transform.position - endTile.transform.position).magnitude);
            costs.Add(t, new Vector2(float.MaxValue, distance));
            parents.Add(t, null);
        }

        openList.Add(startTile);
        Vector2 startCost = costs[startTile];
        startCost.x = 0.0f;
        costs[startTile] = startCost;

        while (openList.Count > 0)
        {
            // Find lowest cost node
            Tile currentTile = GetLowestFCost(costs, openList);

            if (currentTile == endTile) break; // Path finished

            // Remove current node from list and add it to close list
            openList.Remove(currentTile);
            closeList.Add(currentTile);

            // Explore current node neighborhood
            foreach (Tile t in currentTile.neighbours)
            {

                if (closeList.Contains(t)) continue; // Node already visited

                if (!t.IsEmpty() && t != endTile && t != startTile) continue; //Tile is not navigatable

                float newGCost = costs[currentTile].x + (currentTile.transform.position - t.transform.position).magnitude;

                if (newGCost < costs[t].x)
                {
                    parents[t] = currentTile;
                    Vector2 c = costs[t];
                    c.x = newGCost;
                    costs[t] = c;

                    if (!openList.Contains(t))
                        openList.Add(t);
                }

            }

        }

        // Create Path
        List<Tile> pathNodes = new List<Tile>();
        if (parents[endTile] == null || parents[endTile] == endTile)
        {
            Debug.Log("Path not found " + parents[endTile]);
        }
        else
        {
            // Path found
            Tile currentTile = endTile;

            do
            {
                pathNodes.Add(currentTile);
                currentTile = parents[currentTile];
            } while (currentTile != null);

            pathNodes.Reverse();
        }
       
        return pathNodes;
    }

    /// <summary>
    /// Return the index (int) of the node with the lowest F cost in the openList.
    /// </summary>
    /// <param name="costs"> Native array containing the cost of all tiles</param>
    /// <param name="openList"> Native list of the open list of tiles</param>
    /// <returns></returns>
    private Tile GetLowestFCost(Dictionary<Tile, Vector2> costs, List<Tile> openList)
    {
        Tile lowestCostNode = openList[0];

        foreach (Tile t in openList)
        {
            if ((costs[t].x + costs[t].y) < (costs[lowestCostNode].x + costs[lowestCostNode].y))
                lowestCostNode = t;
        }

        return lowestCostNode;
    }

    public Unit GetClosestEnemyUnitTo(Unit _unit)
    {
        List<Tile> enemyPosition = tiles.FindAll(t => !t.IsEmpty() && t.GetUnit().owner != _unit.owner);

        if (enemyPosition.Count == 0) return null;

        float minDistance = float.MaxValue;
        Unit closestUnit = enemyPosition[0].GetUnit();
        foreach (Tile t in enemyPosition)
        {
            float d = (t.pivot - _unit.transform.position).magnitude;
            if (d < minDistance)
            {
                minDistance = d;
                closestUnit = t.GetUnit();
            }
        }

        return closestUnit;

    }
}

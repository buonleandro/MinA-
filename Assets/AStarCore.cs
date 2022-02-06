using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class AStarCore : MonoBehaviour
{
    PathManager manager;
    Grid grid;
    Stopwatch sw;

    void Awake()
    {
        manager = GetComponent<PathManager>();
        grid = GetComponent<Grid>();
    }

    public void StartFind(Vector3 startPos, Vector3 endPos)
    {
        StartCoroutine(FindPath(startPos, endPos));
    }

    IEnumerator FindPath(Vector3 startPos, Vector3 endPos)
    {
        Node start = grid.PositionToNode(startPos);
        Node end = grid.PositionToNode(endPos);
        sw = new Stopwatch();

        Vector3[] waypoints = new Vector3[0];
        bool pathCompleted = false;
        
        MinHeap<Node> openList = new MinHeap<Node>(grid.gridSize);
        HashSet<Node> closedList = new HashSet<Node>();
        openList.Add(start);

        sw.Start();
        while (openList.Count>0)
        {
            Node current = openList.Pop();
            closedList.Add(current);

            if (current == end)
            {
                sw.Stop();
                print("Tempo A*: " + sw.ElapsedMilliseconds + " ms");
                pathCompleted = true;
                break;
            }

            foreach (Node neighbour in grid.NeighboursFromNode(current))
            {
                if (!neighbour.walkable || closedList.Contains(neighbour)) 
                    continue;

                int newCostToNeighbour = current.gCost + GetDistance(current, neighbour) + neighbour.walkCost;
                if (newCostToNeighbour < neighbour.gCost || !openList.Contains(neighbour))
                {
                    neighbour.gCost = newCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, end);
                    neighbour.parent = current;

                    if (!openList.Contains(neighbour))
                    {
                        openList.Add(neighbour);
                    }
                    else
                    {
                        openList.UpdateItem(neighbour);
                    }
                }
            }
        }
        yield return null;
        if (pathCompleted)
        {
            waypoints = TracePath(start,end);
        }
        manager.FinishProcessing(waypoints,pathCompleted);
    }

    Vector3[] TracePath(Node start, Node end)
    {
        List<Node> path = new List<Node>();
        Node current = end;

        while (current != start)
        {
            path.Add(current);
            current = current.parent;
        }

        Vector3[] waypoints = SimplifyPath(path);
        Array.Reverse(waypoints);
        return waypoints;
    }

    Vector3[] SimplifyPath(List<Node> path)
    {
        List<Vector3> waypoints = new List<Vector3>();
        Vector2 directionOld = Vector2.zero;

        for (int i = 1; i < path.Count; i++)
        {
            Vector2 directionNew = new Vector2(path[i - 1].gridX - path[i].gridX, path[i - 1].gridY - path[i].gridY);
            if (directionNew != directionOld)
            {
                waypoints.Add(path[i].center + Vector3.up * 1.5f);
            }

            directionOld = directionNew;
        }

        return waypoints.ToArray();
    }

    int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }
    
}

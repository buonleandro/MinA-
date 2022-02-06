using UnityEngine;

public class Node : IHeapItem<Node>
{

    public bool walkable;
    public Vector3 center;

    public int gridX, gridY;
    public int gCost, hCost;
    public Node parent;
    int heapIndex;
    public int walkCost;

    public Node(bool _walkable, Vector3 _center, int _gridX, int _gridY, int _walkCost)
    {
        walkable = _walkable;
        center = _center;
        gridX = _gridX;
        gridY = _gridY;
        walkCost = _walkCost;
    }

    public int fCost
    {
        get
        {
            return gCost + hCost;
        }
    }

    public int HeapIndex
    {
        get => heapIndex;
        set => heapIndex = value;
    }

    public int CompareTo(Node other)
    {
        int comp = fCost.CompareTo(other.fCost);
        if (comp == 0)
        {
            comp = hCost.CompareTo(other.hCost);
        }

        return -comp;
    }
}

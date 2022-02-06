using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;

public class Grid : MonoBehaviour
{

    public bool displayGrid;
    public LayerMask unwalkable;
    public Vector2 gridDim;
    public float nodeRadius;
    public TerrainType[] walkRegions;
    public int costNearObsatclesIncrease = 10;
    Dictionary<int, int> walkRegionsDict = new Dictionary<int, int>();
    LayerMask walkableMask;
    private GameObject[] obstacles;
    public bool update = false;

    Node[,] grid;

    float nodeDiameter;
    int xTiles, yTiles;

    private int walkCostMin = int.MaxValue;
    private int walkCostMax = int.MinValue;

    void Awake()
    {
        nodeDiameter = nodeRadius * 2;
        xTiles = Mathf.RoundToInt(gridDim.x / nodeDiameter);
        yTiles = Mathf.RoundToInt(gridDim.y / nodeDiameter);

        obstacles = GameObject.FindGameObjectsWithTag("obs");

    }

    void Update()
    {
        foreach (GameObject obstacle in obstacles)
        {
            if (obstacle.transform.hasChanged)
            {
                update = true;
            }
        }

        if (update)
        {
            walkRegionsDict.Clear();
            foreach (TerrainType t in walkRegions)
            {
                walkableMask.value |= t.layerMask.value;
                walkRegionsDict.Add((int) Mathf.Log(t.layerMask.value, 2), t.walkCost);
            }
            BuildGrid();
            update = false;
        }
    }
    
    public int gridSize
    {
        get
        {
            return xTiles * yTiles;
        }
    }

    void BuildGrid() {
        grid = new Node[xTiles,yTiles];
        Vector3 bLeftCorner = transform.position - Vector3.right * gridDim.x/2 - Vector3.forward * gridDim.y/2;

        for (int x = 0; x < xTiles; x ++) {
            for (int y = 0; y < yTiles; y ++) {
                Vector3 nodePos = bLeftCorner + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
                bool walkable = !(Physics.CheckSphere(nodePos,nodeRadius,unwalkable));

                int walkCost = 0;

                Ray stantz = new Ray(nodePos + Vector3.up * 50, Vector3.down);
                RaycastHit hit;
                if (Physics.Raycast(stantz, out hit, 100, walkableMask)) 
                { 
                    walkRegionsDict.TryGetValue(hit.collider.gameObject.layer, out walkCost);
                }

                if (!walkable)
                {
                    walkCost += costNearObsatclesIncrease;
                }
                
                grid[x,y] = new Node(walkable,nodePos,x,y,walkCost);
            }
        }
        
        BoxBlur(3);
        
    }

    public List<Node> NeighboursFromNode(Node node)
    {
        List<Node> neighbours = new List<Node>();
        
        for(int x = -1;x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) 
                    continue;

                int neighbourX = node.gridX + x;
                int neighbourY = node.gridY + y;

                if (neighbourX >= 0 && neighbourY >= 0 && neighbourX < xTiles && neighbourY < yTiles)
                {
                    neighbours.Add(grid[neighbourX,neighbourY]);
                }
            }
        }

        return neighbours;
    }
    
    public Node PositionToNode(Vector3 worldPosition) {
        float percentX = (worldPosition.x + gridDim.x/2) / gridDim.x;
        float percentY = (worldPosition.z + gridDim.y/2) / gridDim.y;
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((xTiles-1) * percentX);
        int y = Mathf.RoundToInt((yTiles-1) * percentY);
        return grid[x,y];
    }
    
    void BoxBlur(int blurSize) {
        int kernelSize = blurSize * 2 + 1;
        int kernelExtents = (kernelSize - 1) / 2;

        int[,] horizontalPass = new int[xTiles,yTiles];
        int[,] verticalPass = new int[xTiles,yTiles];

        for (int y = 0; y < yTiles; y++) {
            for (int x = -kernelExtents; x <= kernelExtents; x++) {
                int sampleX = Mathf.Clamp (x, 0, kernelExtents);
                horizontalPass [0, y] += grid [sampleX, y].walkCost;
            }

            for (int x = 1; x < xTiles; x++) {
                int removeIndex = Mathf.Clamp(x - kernelExtents - 1, 0, xTiles);
                int addIndex = Mathf.Clamp(x + kernelExtents, 0, xTiles-1);

                horizontalPass [x, y] = horizontalPass [x - 1, y] - grid [removeIndex, y].walkCost + grid [addIndex, y].walkCost;
            }
        }
			
        for (int x = 0; x < xTiles; x++) {
            for (int y = -kernelExtents; y <= kernelExtents; y++) {
                int sampleY = Mathf.Clamp (y, 0, kernelExtents);
                verticalPass [x, 0] += horizontalPass [x, sampleY];
            }

            int blurredValue = Mathf.RoundToInt((float)verticalPass [x, 0] / (kernelSize * kernelSize));
            grid [x, 0].walkCost = blurredValue;

            for (int y = 1; y < yTiles; y++) {
                int removeIndex = Mathf.Clamp(y - kernelExtents - 1, 0, yTiles);
                int addIndex = Mathf.Clamp(y + kernelExtents, 0, yTiles-1);

                verticalPass [x, y] = verticalPass [x, y-1] - horizontalPass [x,removeIndex] + horizontalPass [x, addIndex];
                blurredValue = Mathf.RoundToInt((float)verticalPass [x, y] / (kernelSize * kernelSize));
                grid [x, y].walkCost = blurredValue;

                if (blurredValue > walkCostMax) {
                    walkCostMax = blurredValue;
                }
                if (blurredValue < walkCostMin) {
                    walkCostMin = blurredValue;
                }
            }
        }

    }
    
    void OnDrawGizmos() {
        Gizmos.DrawWireCube(transform.position,new Vector3(gridDim.x,1,gridDim.y));

        if (grid != null && displayGrid) {
            foreach (Node n in grid)
            {
                Gizmos.color = Color.Lerp(Color.white, Color.black, Mathf.InverseLerp(walkCostMin, walkCostMax, n.walkCost));
                Gizmos.color = (n.walkable)?Gizmos.color:Color.red;
                Gizmos.DrawCube(n.center, Vector3.one * (nodeDiameter-.1f));
            }
        }
    }

    [System.Serializable]
    public class TerrainType
    {
        public LayerMask layerMask;
        public int walkCost;
    }
}

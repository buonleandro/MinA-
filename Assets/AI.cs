using UnityEngine;
using UnityEngine.AI;

public class AI : MonoBehaviour
{
    public GameObject[] paths;

    public NavMeshAgent agent;

    private int startingPath = 0;
    private int pathLength = 0;

    private void Start()
    {
        startingPath = 0;
        pathLength = paths.Length;
        agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        if (startingPath < pathLength)
        {
            if (Vector3.Distance(paths[startingPath].transform.position, gameObject.transform.position) < 1)
            {
                if (startingPath == pathLength - 1)
                {
                    startingPath = 0;
                }
                else
                {
                    startingPath++;
                }
            }
        }

        agent.SetDestination(paths[startingPath].transform.position);
    }
}
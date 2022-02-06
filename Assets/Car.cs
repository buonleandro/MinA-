using System.Collections;
using UnityEngine;

public class Car : MonoBehaviour
{
    private float moveThreshold = .5f;

    public bool drawWaypoints;
    public Transform target;
    public float speed = 8;
    public float turnDistance = 5;
    private float turnSpeed;
    public float stoppingDistance = 10;
    private GameObject[] obstacles;
    public bool update = false;

    Path path;

    void Start()
    {
        turnSpeed = speed -5;
        obstacles = GameObject.FindGameObjectsWithTag("obs");
        StartCoroutine("UpdatePath");
    }

    IEnumerator UpdatePath()
    {
        if (Time.timeSinceLevelLoad < .3f)
        {
            yield return new WaitForSeconds(.3f);
        }
        
        PathManager.Request(transform.position,target.position,OnPathFound);

        float sqrThreshold = moveThreshold * moveThreshold;
        Vector3 actualTargetPos = target.position;
        
        while (true)
        {
            foreach (GameObject obstacle in obstacles)
            {
                if (obstacle.transform.hasChanged)
                {
                    update = true;
                    obstacle.transform.hasChanged = false;
                }
            }
            yield return new WaitForSeconds(.6f);
            if ((target.position - actualTargetPos).sqrMagnitude > sqrThreshold||update)
            {
                PathManager.Request(transform.position,target.position,OnPathFound);
                actualTargetPos = target.position;
                if (update)
                {
                    update = false;
                }
            }
        }
    }

    public void OnPathFound(Vector3[] _path, bool pathCompleted)
    {
        if (pathCompleted)
        {
            path = new Path(_path, transform.position, turnDistance, stoppingDistance);
            StopCoroutine("MoveToTarget");
            StartCoroutine("MoveToTarget");
        }
    }

    IEnumerator MoveToTarget()
    {
        bool followingPath = true;
        int pathIndex = 0;

        float speedPercent = 1;
        
        transform.LookAt(path.lookPoints[0]);

        while (followingPath)
        {
            Vector2 v2Pos = new Vector2(transform.position.x, transform.position.z);
            while (path.turnBoundaries[pathIndex].HasCrossedLine(v2Pos))
            {
                if (pathIndex == path.finishLineIndex)
                {
                    followingPath = false;
                    break;
                }
                else
                {
                    pathIndex++;
                }
            }

            if (followingPath)
            {

                if (pathIndex >= path.slowDownIndex && stoppingDistance > 0)
                {
                    speedPercent = Mathf.Clamp01(path.turnBoundaries[path.finishLineIndex].DistanceFromPoint(v2Pos) /
                                   stoppingDistance);
                    if (speedPercent < 0.01f)
                    {
                        followingPath = false;
                    }
                }

                Quaternion finalRotation = Quaternion.LookRotation(path.lookPoints[pathIndex] - transform.position);
                transform.rotation = Quaternion.Lerp(transform.rotation,finalRotation,Time.deltaTime*turnSpeed);
                transform.Translate(Vector3.forward*Time.deltaTime*speed*speedPercent,Space.Self);

            }

            yield return null;

        }

    }

    private void OnDrawGizmos()
    {
        if (path != null && drawWaypoints)
        {
           path.DrawWithGizmos();
        }
    }
}

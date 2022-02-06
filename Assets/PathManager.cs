using System;
using System.Collections.Generic;
using UnityEngine;

public class PathManager : MonoBehaviour
{
    
    Queue<PathRequest> pathRequests = new Queue<PathRequest>();
    PathRequest currentRequest;

    static PathManager instance;
    AStarCore aStar;

    bool isProcessing;

    void Awake()
    {
        instance = this;
        aStar = GetComponent<AStarCore>();
    }

    public static void Request(Vector3 startPos, Vector3 endPos, Action<Vector3[], bool> callback)
    {
        PathRequest newRequest = new PathRequest(startPos, endPos, callback);
        instance.pathRequests.Enqueue(newRequest);
        instance.ProcessNextRequest();
    }

    void ProcessNextRequest()
    {
        if (!isProcessing && pathRequests.Count > 0)
        {
            currentRequest = instance.pathRequests.Dequeue();
            isProcessing = true;
            aStar.StartFind(currentRequest.startPos, currentRequest.endPos);
        }
    }

    public void FinishProcessing(Vector3[] path, bool pathCompleted)
    {
        currentRequest.callback(path, pathCompleted);
        isProcessing = false;
        ProcessNextRequest();
    }
    
    struct PathRequest
    {
        public Vector3 startPos;
        public Vector3 endPos;
        public Action<Vector3[], bool> callback;

        public PathRequest(Vector3 _startPos, Vector3 _endPos, Action<Vector3[], bool> _callback)
        {
            startPos = _startPos;
            endPos = _endPos;
            callback = _callback;
        }

    }
    
}

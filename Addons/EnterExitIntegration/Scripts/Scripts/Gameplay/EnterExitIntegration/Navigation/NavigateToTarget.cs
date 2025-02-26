using System;
using System.Collections.Generic;
using UnityEngine;
public class NavigateToTarget : MonoBehaviour
{
    [SerializeField] private Transform[] nodes;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Transform finalRotationAtDestination;

    internal Transform toMove;
    internal Transform destination;

    private bool startMoving;
    private int pathIndex = 0;
    private Transform closestNode;
    private List<Transform> shortestPath = new List<Transform>();

    public static event Action OnFindShortestPathAction;
    public static event Action OnDestinationReachedAction;

    public void FindTheShortestPath()
    {
        if (toMove == null || destination == null || nodes == null || nodes.Length == 0)
            return;

        OnFindShortestPathAction?.Invoke();

        startMoving = false;

        closestNode = nodes[0];
        int closestIndex = 0;
        for (int i = 0; i < nodes.Length; i++)
        {
            if (Vector3.Distance(toMove.position, nodes[i].position) < Vector3.Distance(toMove.position, closestNode.position))
            {
                closestNode = nodes[i];
                closestIndex = i;
            }
        }

        float forwardDistance = 0f;
        for (int i = closestIndex; i < nodes.Length - 1; i++)
        {
            forwardDistance += Vector3.Distance(nodes[i].position, nodes[i + 1].position);
        }
        forwardDistance += Vector3.Distance(nodes[nodes.Length - 1].position, destination.position);

        float backwardDistance = 0f;
        for (int i = closestIndex; i > 0; i--)
        {
            backwardDistance += Vector3.Distance(nodes[i].position, nodes[i - 1].position);
        }
        backwardDistance += Vector3.Distance(nodes[0].position, destination.position);

        shortestPath.Clear();
        if (forwardDistance < backwardDistance)
        {
            for (int i = closestIndex; i < nodes.Length; i++)
            {
                shortestPath.Add(nodes[i]);
            }
        }
        else
        {
            for (int i = closestIndex; i >= 0; i--)
            {
                shortestPath.Add(nodes[i]);
            }
        }

        startMoving = true;
        pathIndex = 0;
    }

    private void Update()
    {
        if (!startMoving || toMove == null) return;

        if (pathIndex < shortestPath.Count)
        {
            Transform currentTarget = shortestPath[pathIndex];
            Vector3 targetPos = new Vector3(currentTarget.position.x, toMove.position.y, currentTarget.position.z);

            if (Vector3.Distance(toMove.position, targetPos) < 0.2f)
            {
                pathIndex++;
            }
            else
            {
                MoveTowardsTarget(currentTarget);
            }
        }
        else
        {
            MoveTowardsTarget(destination);
            if (Vector3.Distance(toMove.position, new Vector3(destination.position.x, toMove.position.y, destination.position.z)) < 0.05f)
            {
                FinalizeMovement();
            }
        }
    }

    private void MoveTowardsTarget(Transform target)
    {
        Vector3 targetPos = new Vector3(target.position.x, toMove.position.y, target.position.z);
        Quaternion lookRotation = Quaternion.LookRotation(targetPos - toMove.position);
        float rotationSpeed = Mathf.Clamp(moveSpeed * 0.5f, 2f, 15f);
        toMove.rotation = Quaternion.Slerp(toMove.rotation, lookRotation, rotationSpeed * Time.deltaTime);
        toMove.position = Vector3.MoveTowards(toMove.position, targetPos, moveSpeed * Time.deltaTime);
    }

    private void FinalizeMovement()
    {
        toMove.position = new Vector3(destination.position.x, toMove.position.y, destination.position.z);
        startMoving = false;
        pathIndex = 0;

        if (toMove != null && finalRotationAtDestination != null)
        {
            toMove.rotation = finalRotationAtDestination.rotation; // Instantly set rotation
        }

        OnDestinationReachedAction?.Invoke();
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (nodes == null || nodes.Length == 0) return;

        Gizmos.color = Color.yellow;
        foreach (Transform node in nodes)
        {
            if (node != null)
                Gizmos.DrawSphere(node.position, 0.2f);
        }

        if (shortestPath != null && shortestPath?.Count > 0)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < shortestPath.Count - 1; i++)
            {
                if (shortestPath[i] != null && shortestPath[i + 1] != null)
                    Gizmos.DrawLine(shortestPath[i].position, shortestPath[i + 1].position);
            }

            if (destination != null && shortestPath[shortestPath.Count - 1] != null)
                Gizmos.DrawLine(shortestPath[shortestPath.Count - 1].position, destination.position);
        }
    }
#endif
}

﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof (LineRenderer))]
[RequireComponent (typeof (Rigidbody2D))]
public class Hook : MonoBehaviour {

	public LineRenderer lr;

    [SerializeField]
    private LayerMask hookableMask;
    [SerializeField]
    private float hookSpeed = 5f;
    private Player player;
    private Rigidbody2D rb;
    private Collider2D col;
    private List<Vector3> ropePoints = new List<Vector3> ();
    private Vector3 currentHookPoint;
    private Vector3 connectedAnchorCenter;
    private Vector3 previousConnectedAnchorCenter;
    private float minAngleDifference = 1f;
    // This is the distance to begin the collision raycast taken from the currentHookPoint in the direction away from the conrer..
    // This stops the raycast detecting only the currentHookPoint
    private float hookRaycastOffset = 0.2f;
    private int numHookEdgeIterations = 10;
    public bool isLanded;

    private void Awake ()
    {
        player = FindObjectOfType<Player> ();
		lr = GetComponent<LineRenderer> ();
        rb = GetComponent<Rigidbody2D> ();
        col = GetComponent<Collider2D> ();
    }

    private void Start ()
    {
        lr.enabled = false;
    }

    private void Update ()
    {
        if (isLanded)
        {
            DetectRopeCollisions ();
            if (ropePoints.Count > 2)
            {
                DetectRopeSeparations ();
            }
        }
    }

    private void LateUpdate ()
    {
        if (isLanded)
        {
            DrawRope ();
        }
    }
	private void OnTriggerEnter2D (Collider2D other)
    {
        if (other.gameObject.tag == "ground" && player.isFiring)
        {
            lr.enabled = true;
            isLanded = true;
            connectedAnchorCenter = other.bounds.center;
            Vector3 roundedHookPosition = new Vector3 (Mathf.Round (transform.position.x * 2) / 2, Mathf.Round (transform.position.y * 2) / 2, 0);
            if (player.transform.position.y < other.bounds.center.y)
            {
                roundedHookPosition.y = other.bounds.min.y;
            }
            if (player.transform.position.y > other.bounds.center.y)
            {
                roundedHookPosition.y = other.bounds.max.y;
            }
            transform.position = roundedHookPosition;
            currentHookPoint = roundedHookPosition;
            player.HookLanded (currentHookPoint);
            ropePoints.Add (transform.position);
            ropePoints.Add (player.HookStartPoint.position);
            col.enabled = false;
        }
    }

    public void Fire (Vector3 startPos, Vector3 endPos)
    {
        StartCoroutine (FireRoutine (startPos, endPos));
    }

    private IEnumerator FireRoutine (Vector3 startPos, Vector3 endPos)
    {
        Vector3 direction = endPos - startPos;
        while (transform.position != endPos && !isLanded)
        {
            // transform.Translate (direction * Time.deltaTime * hookSpeed);
            rb.MovePosition (new Vector3 (rb.position.x, rb.position.y, 0) + direction * Time.deltaTime * hookSpeed);
            yield return null;
        }
    }

    public void ClearRopePoints ()
    {
        ropePoints.Clear ();
        lr.numPositions = 0;
        col.enabled = true;
    }

	private void DrawRope ()
	{
        lr.numPositions = ropePoints.Count;

		lr.SetPosition (0, transform.position + Vector3.back);

        for (int i = 1; i < ropePoints.Count - 1; i++)
        {
            lr.SetPosition (i, ropePoints [i]);
        }
		lr.SetPosition (lr.numPositions - 1, player.HookStartPoint.position + Vector3.back);

	}

    private void DetectRopeCollisions ()
    {
        // Vector2 raycastOrigin = transform.position + hookRaycastOffset * (player.HookStartPoint.position - transform.position);
        Vector2 relativeRaycastOrigin = Vector2.zero;
        relativeRaycastOrigin.x = currentHookPoint.x < connectedAnchorCenter.x ? -hookRaycastOffset : hookRaycastOffset;
        relativeRaycastOrigin.y = currentHookPoint.y < connectedAnchorCenter.y ? -hookRaycastOffset : hookRaycastOffset;

        Vector2 raycastOrigin = (Vector2)currentHookPoint + relativeRaycastOrigin;

        RaycastHit2D hit = Physics2D.Raycast (
            raycastOrigin, 
            player.HookStartPoint.position - currentHookPoint, 
            Mathf.Abs (Vector2.Distance (currentHookPoint, player.HookStartPoint.position)),
            hookableMask
        );
		
        // Debug.DrawRay (raycastOrigin, player.HookStartPoint.position - currentHookPoint, Color.blue, 2f);

        if (hit.collider != null)
		{
            if (hit.point != (Vector2)currentHookPoint)
            {
            Debug.DrawLine (currentHookPoint, hit.point, Color.red, 5f);
                if (hit.collider.gameObject.tag == "ground")
                {
                    FindHookEdgePoint (currentHookPoint, hit.point, hit.collider, player.Velocity, numHookEdgeIterations);
                    Vector3 newHookPoint = Vector3.zero;
                    newHookPoint.x = hit.point.x < hit.collider.bounds.center.x ? hit.collider.bounds.min.x : hit.collider.bounds.max.x;
                    newHookPoint.y = hit.point.y < hit.collider.bounds.center.y ? hit.collider.bounds.min.y : hit.collider.bounds.max.y;
                    if (ropePoints.Count == 2)
                    {
                        previousConnectedAnchorCenter = connectedAnchorCenter;
                    }
                    connectedAnchorCenter = hit.collider.bounds.center;
                    Debug.DrawLine (currentHookPoint, newHookPoint, Color.red, 5f);
                    AddNodeToRope (newHookPoint);
                }
            }
		}
    }

    private void DetectRopeSeparations ()
    {
        Vector3 playerToCurrentAnchor = player.HookStartPoint.position - ropePoints [ropePoints.Count - 2];
        Vector3 playerToPreviousAnchor = player.HookStartPoint.position - ropePoints [ropePoints.Count - 3];
        Vector3 currentAnchorToPreviousAnchor = ropePoints [ropePoints.Count - 2] - ropePoints [ropePoints.Count - 3];

        // Ensure the angles between points are greater or equal to 0 and less than to 360
        float playerToCurrentAnchorAngle = (Utilities.AngleFromVector3 (playerToCurrentAnchor) + 360) % 360;
        float playerToPreviousAnchorAngle = (Utilities.AngleFromVector3 (playerToPreviousAnchor) + 360) % 360;
        float currentAnchorToPreviousAnchorAngle = (Utilities.AngleFromVector3 (currentAnchorToPreviousAnchor) + 360) % 360;

        // Check for swinging direction
        if (IsSwingingClockwise ())
        {
            playerToCurrentAnchorAngle = playerToCurrentAnchorAngle == 0 ? 360 : playerToCurrentAnchorAngle;
            currentAnchorToPreviousAnchorAngle = currentAnchorToPreviousAnchorAngle == 0 ? 360 : currentAnchorToPreviousAnchorAngle;
            if (playerToCurrentAnchorAngle < currentAnchorToPreviousAnchorAngle && playerToCurrentAnchorAngle > (currentAnchorToPreviousAnchorAngle + 270) % 360)
            {
                RemoveNodeFromRope ();
            }
        }
        else
        {
            if (playerToCurrentAnchorAngle > currentAnchorToPreviousAnchorAngle && (playerToCurrentAnchorAngle - 90) % 360 < currentAnchorToPreviousAnchorAngle)
            {
                RemoveNodeFromRope ();
            }
        }
    }

    private void AddNodeToRope (Vector3 newNode)
    {
        currentHookPoint = newNode;
        player.HookLanded (newNode);
        ropePoints.Insert (ropePoints.Count - 1, newNode);

        // RaycastHit2D hit = Physics2D.Raycast (
        //     newNode, 
        //     ropePoints [ropePoints.Count - 2] - newNode, 
        //     Mathf.Abs (Vector2.Distance (ropePoints [ropePoints.Count - 2], newNode)),
        //     hookableMask
        // );
        Debug.DrawLine (
            newNode, 
            ropePoints [ropePoints.Count - 2],
            Color.green,
            2f
        );
    }

    private void RemoveNodeFromRope ()
    {
        ropePoints.RemoveAt (ropePoints.Count - 2);
        currentHookPoint = ropePoints [ropePoints.Count - 2];
        if (ropePoints.Count == 2)
        {
            connectedAnchorCenter = previousConnectedAnchorCenter;
        }
        player.HookLanded (currentHookPoint);
    }

    private Vector3 FindHookEdgePoint (Vector3 currentAnchor, Vector3 triggerPoint, Collider2D triggerCollider, Vector3 playerVelocity, int numInterations, float initialDegreesOffset = 5f)
    {
        Vector3 edgePoint = Vector3.zero;
        Vector3 currentHookPointToTriggerPoint = triggerPoint - currentAnchor;
        Vector3 directionToTriggerPoint = currentHookPointToTriggerPoint.normalized;

        Vector3 previousUnitPlayerPos = player.HookStartPoint.position - playerVelocity.normalized;
        Vector3 currentHookPointToPreviousPlayerPos = triggerPoint - previousUnitPlayerPos;


        Vector3 minPos = previousUnitPlayerPos;
        Vector3 maxPos = triggerPoint;
        Vector3 targetPos = (minPos + maxPos) / 2f;

        for (int i = 0; i < numInterations; i++)
        {
            RaycastHit2D findEdgeRay = Physics2D.Raycast (
                currentHookPoint, 
                targetPos - currentHookPoint,
                Mathf.Abs (Vector2.Distance (currentHookPoint, player.HookStartPoint.position)),
                hookableMask
            );

            if (findEdgeRay.collider != triggerCollider)
            // Ray is no longer hitting the same collider as trigger, set target to halway between max and min points
            {
                
            }
        }

        return edgePoint;
    }

    private bool IsSwingingClockwise ()
    {

        Vector3 currentAnchorPoint = ropePoints [ropePoints.Count - 2];
        Vector3 previousAnchorPoint = ropePoints [ropePoints.Count - 3];

        Vector3 midPoint = (currentAnchorPoint + previousAnchorPoint) / 2;
        int direction = (int)Utilities.AngleFromVector3 (previousAnchorPoint - currentAnchorPoint);
        
        if (direction < 0)
        {
            direction += 360;
        }

        // Right
        if (direction == 0 || direction > 270)
        {
            if (currentAnchorPoint.y < connectedAnchorCenter.y)
            {
                return false;
            }
            return true;
        }
        // Up
        else if (direction <= 90)
        {
            if (currentAnchorPoint.x > connectedAnchorCenter.x)
            {
                return false;
            }
            return true;
        }
        // Left
        else if (direction <= 180)
        {
            if (currentAnchorPoint.y > connectedAnchorCenter.y)
            {
                return false;
            }
            return true;
        // Down
        }
        else if (direction <= 270)
        {
            if (currentAnchorPoint.x < connectedAnchorCenter.x)
            {
                return false;
            }
            return true;
        }
        return false;
    }

}

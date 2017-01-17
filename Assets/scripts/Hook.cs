using UnityEngine;
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
    private List<Vector3> ropePoints = new List<Vector3> ();
    private Vector3 currentHookPoint;
    private Vector3 connectedAnchorCenter;
    // This is the distance to begin the collision raycast taken from the currentHookPoint in the direction away from the conrer..
    // This stops the raycast detecting only the currentHookPoint
    private float hookRaycastOffset = 0.2f;

    public bool isLanded;

    private void Awake ()
    {
        player = FindObjectOfType<Player> ();
		lr = GetComponent<LineRenderer> ();
        rb = GetComponent<Rigidbody2D> ();
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
            player.HookLanded (roundedHookPosition);
            currentHookPoint = roundedHookPosition;
            ropePoints.Add (transform.position);
            ropePoints.Add (player.HookStartPoint.position);
            // lr.numPositions = 2;
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
    }

	private void DrawRope ()
	{
        lr.numPositions = ropePoints.Count;

		lr.SetPosition (0, transform.position + Vector3.back);

        for (int i = 1; i < ropePoints.Count - 1; i++)
        {
            lr.SetPosition (i, ropePoints [i - 1]);
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
		
        Debug.DrawRay (raycastOrigin, player.HookStartPoint.position - currentHookPoint, Color.blue, 2f);

        if (hit.collider != null)
		{
            if (hit.point != (Vector2)currentHookPoint)
            {
            Debug.DrawLine (currentHookPoint, hit.point, Color.red, 5f);
                if (hit.collider.gameObject.tag == "ground")
                {
                    Vector3 newHookPoint = Vector3.zero;
                    newHookPoint.x = hit.point.x < hit.collider.bounds.center.x ? hit.collider.bounds.min.x : hit.collider.bounds.max.x;
                    newHookPoint.y = hit.point.y < hit.collider.bounds.center.y ? hit.collider.bounds.min.y : hit.collider.bounds.max.y;
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
        Vector3 currentAnchorToPreviousAnchor = ropePoints [ropePoints.Count - 2] - ropePoints [ropePoints.Count - 3];
        float angleDifference = Vector3.Angle (playerToCurrentAnchor, currentAnchorToPreviousAnchor);

        if (angleDifference > 179f)
        {
            RemoveNodeFromRope ();
        }
        
    }

    private void AddNodeToRope (Vector3 newNode)
    {
        currentHookPoint = newNode;
        player.HookLanded (newNode);
        ropePoints.Insert (ropePoints.Count - 2, newNode);
    }

    private void RemoveNodeFromRope ()
    {
        ropePoints.RemoveAt (ropePoints.Count - 1);
        currentHookPoint = ropePoints [ropePoints.Count - 1];
        player.HookLanded (currentHookPoint);
    }



}

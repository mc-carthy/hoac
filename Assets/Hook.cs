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
    private List<Vector3> staticRopePoints = new List<Vector3> ();

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
        DetectRopeCollisions ();
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
        Debug.Log ("Test");
        if (other.gameObject.tag == "ground" && player.isFiring)
        {
            lr.enabled = true;
            isLanded = true;
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
            player.HookLanded (transform.position);
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
        staticRopePoints.Clear ();
    }

	private void DrawRope ()
	{
		lr.SetPosition (0, transform.position + Vector3.back);

        if (lr.numPositions > 2)
        {
            for (int i = 1; i < lr.numPositions - 1; i++)
            {
                lr.SetPosition (i, staticRopePoints [i - 1]);
            }
        }

		lr.SetPosition (lr.numPositions - 1, player.HookStartPoint.position + Vector3.back);
	}

    private void DetectRopeCollisions ()
    {
		RaycastHit2D hit = Physics2D.Raycast (transform.position, player.HookStartPoint.position - transform.position, Mathf.Infinity, hookableMask);

		if (hit.collider != null)
		{
			if (hit.collider.gameObject.tag == "ground")
			{
                AddNodeToRope (new Vector3 (hit.point.x, hit.point.y, 0));
			}
		}
    }

    private void AddNodeToRope (Vector3 newNode)
    {
    }

}

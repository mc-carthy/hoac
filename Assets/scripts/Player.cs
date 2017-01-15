using UnityEngine;

[RequireComponent (typeof (DistanceJoint2D))]
[RequireComponent (typeof (LineRenderer))]
public class Player : MonoBehaviour {

	[SerializeField]
	private float moveForce = 500;
	[SerializeField]
	private float jumpForce = 500;
	[SerializeField]
	private float hookReelSpeed = 5;
	private Rigidbody2D rb;
	private DistanceJoint2D dj;
	private LineRenderer lr;
	private Vector3 hookPoint;
	private bool isHooked = false;
	private bool isJumping = false;

	private void Awake ()
	{
		rb = GetComponent<Rigidbody2D> ();
		dj = GetComponent<DistanceJoint2D> ();
		lr = GetComponent<LineRenderer> ();
	}

	private void Start ()
	{
		dj.enabled = false;
	}

	private void Update ()
	{
		MovePlayer ();
		if (Input.GetKeyDown (KeyCode.Space) && isJumping == false)
		{
			Jump ();
		}

		if (Input.GetKeyDown (KeyCode.Mouse0) && isHooked == false)
		{
			FireHook ();
		}

		if (isHooked)
		{
			AlterHookDistance ();
		}

		if (Input.GetKeyUp (KeyCode.Mouse0) && isHooked == true)
		{
			RetractHook ();
		}
	}

	private void LateUpdate ()
	{
		if (isHooked)
		{
			DrawRope ();
		}
	}

	private void OnCollisionEnter2D (Collision2D other)
	{
		if (other.gameObject.tag == "ground")
		{
			isJumping = false;
		}
	}

	private void OnCollisionExit2D (Collision2D other)
	{
		if (other.gameObject.tag == "ground")
		{
			isJumping = true;
		}
	}

	private void MovePlayer ()
	{
		rb.AddForce (Vector2.right * Input.GetAxisRaw ("Horizontal") * moveForce * Time.deltaTime);
	}

	private void Jump ()
	{
		rb.AddForce (Vector2.up * jumpForce);
	}

	private void FireHook ()
	{
		hookPoint = Camera.main.ScreenToWorldPoint (Input.mousePosition);
		hookPoint.z = 0;

		Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
		RaycastHit2D hit = Physics2D.Raycast (ray.origin, ray.direction, Mathf.Infinity);

		if (hit.collider != null)
		{
			if (hit.collider.gameObject.tag == "ground")
			{
				dj.connectedAnchor = hookPoint;
				dj.distance = Vector2.Distance (transform.position, hookPoint);
				isHooked = true;
				dj.enabled = true;
				lr.enabled = true;
			}
		}
	}

	private void AlterHookDistance ()
	{
		dj.distance += Input.GetAxis ("Vertical") * -hookReelSpeed * Time.deltaTime;
	}

	private void RetractHook ()
	{
		isHooked = false;
		dj.enabled = false;
		lr.enabled = false;
	}

	private void DrawRope ()
	{
		lr.numPositions = 2;
		lr.SetPosition (0, transform.position + Vector3.back);
		lr.SetPosition (1, hookPoint + Vector3.back);
		
	}

}

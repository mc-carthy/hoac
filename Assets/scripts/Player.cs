using UnityEngine;

[RequireComponent (typeof (DistanceJoint2D))]
public class Player : MonoBehaviour {

	public Vector3 Velocity {
		get {
			return rb.velocity;
		}
	}

	[SerializeField]
	private Transform hookStartPoint;
	public Transform HookStartPoint 
	{ 
		get { return hookStartPoint; } 
	}

	[SerializeField]
	private Hook hookPrefab;
	[SerializeField]
	private float moveForce = 500;
	[SerializeField]
	private float jumpForce = 500;
	[SerializeField]
	private float hookReelSpeed = 5;
	[SerializeField]
	private LayerMask hookableMask;
	private Hook hook;
	private Rigidbody2D rb;
	private DistanceJoint2D dj;
	private Vector3 hookPoint;
	private bool isHooked = false;
	public bool isFiring = false;
	private bool isJumping = false;
	private float hookMinDist = 1f;
	private float hookMaxDist = 30f;

	private void Awake ()
	{
		rb = GetComponent<Rigidbody2D> ();
		dj = GetComponent<DistanceJoint2D> ();
		hook = Instantiate (hookPrefab, hookStartPoint.position, Quaternion.identity) as Hook;
	}

	private void Start ()
	{
		dj.enabled = false;
	}

	private void Update ()
	{
		MovePlayer ();

		if (Input.GetKeyDown (KeyCode.Space))
		{
			if (!isJumping && !isHooked)
			{
				Jump ();
			}
			if (isHooked)
			{
				RetractHook ();
			}
		}

		if (Input.GetKeyDown (KeyCode.Mouse0) && isHooked == false)
		{
			ShootHook ();
		}

		if (isHooked)
		{
			AlterHookDistance ();
		}
		else
		{
			if (!isFiring)
			{
				hook.transform.position = hookStartPoint.position;
			}
		}

		if (Input.GetKeyUp (KeyCode.Mouse0) && isHooked == true)
		{
			RetractHook ();
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

	private void ShootHook ()
	{
		hookPoint = Camera.main.ScreenToWorldPoint (Input.mousePosition);
		hookPoint.z = 0;

		RaycastHit2D hit = Physics2D.Raycast (hookStartPoint.position, hookPoint - hookStartPoint.position, Mathf.Infinity, hookableMask);

		if (hit.collider != null)
		{
			if (hit.collider.gameObject.tag == "ground")
			{
				FireHook ();
				isFiring = true;
			}
		}
	}

	private void FireHook ()
	{
		hook.Fire (hookStartPoint.position, hookPoint);
	}

	public void HookLanded (Vector3 hookLandedPoint)
	{
		dj.connectedAnchor = (Vector2)hookLandedPoint;
		dj.distance = Vector2.Distance (transform.position, hookLandedPoint);
		hookPoint.x = hookLandedPoint.x;
		hookPoint.y = hookLandedPoint.y;
		isHooked = true;
		isFiring = false;
		dj.enabled = true;
	}

	private void AlterHookDistance ()
	{
		if (IsAreaAroundTheHookedPlayerClear (1f) != 0)
		{
			dj.distance += IsAreaAroundTheHookedPlayerClear (1f);
		}
		if (IsAreaAroundTheHookedPlayerClear (-1f) != 0)
		{
			dj.distance -= IsAreaAroundTheHookedPlayerClear (-1f);
		}

		dj.distance += Input.GetAxis ("Vertical") * -hookReelSpeed * Time.deltaTime;
		dj.distance = Mathf.Clamp (dj.distance, hookMinDist, hookMaxDist);

	}

	private float IsAreaAroundTheHookedPlayerClear (float sign, float distance = 1f)
	{
		Vector3 ropeVector = (Vector3)dj.connectedAnchor - hookStartPoint.position;
		ropeVector.Normalize ();

		RaycastHit2D checkCollisionRay = Physics2D.Raycast (
			hookStartPoint.position, 
			ropeVector * sign,
			distance,
			hookableMask
		);

		if (checkCollisionRay.collider != null)
		{
			// Debug.DrawLine (hookStartPoint.position, checkCollisionRay.point, Color.red, 5f);
			return distance - Vector3.Distance (checkCollisionRay.point, hookStartPoint.position);
		}

		// Debug.DrawRay (
		// 	hookStartPoint.position,
		// 	ropeVector * sign,
		// 	Color.blue,
		// 	1f
		// );

		return 0f;

	}

	private void RetractHook ()
	{
		isHooked = false;
		hook.isLanded = false;
		dj.enabled = false;
		hook.lr.enabled = false;
		hook.ClearRopePoints ();
	}

}

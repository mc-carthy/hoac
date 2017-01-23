using UnityEngine;

public class Utilities : MonoBehaviour {

	static public float AngleFromVector3 (Vector3 vector)
    {
        return Mathf.Atan2 (vector.y, vector.x);
    }

}

using UnityEngine;

public class CameraController : MonoBehaviour {

    private float cameraMoveSpeed = 2.5f;
    private Vector3 pos;
    
    private GameObject player;

    private void Awake ()
    {
        player = GameObject.FindGameObjectWithTag ("Player");
    }

    private void Start ()
    {
        pos = transform.position;
    }

	private void Update ()
    {
        float playerX = player.transform.position.x;

        pos.x = Mathf.Lerp (transform.position.x, playerX, Time.deltaTime * cameraMoveSpeed);

        transform.position = pos;
    }

}

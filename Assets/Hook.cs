using UnityEngine;
using System.Collections;

public class Hook : MonoBehaviour {

    [SerializeField]
    private float hookSpeed = 5f;
    private Player player;
    public bool isLanded;

    private void Awake ()
    {
        player = FindObjectOfType<Player> ();
    }

	private void OnTriggerEnter2D (Collider2D other)
    {
        Debug.Log ("Hit " + other.gameObject.name);
        if (other.gameObject.tag == "ground" && player.isFiring)
        {
            isLanded = true;
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
            transform.Translate (direction * Time.deltaTime * hookSpeed);
            yield return null;
        }
    }

}

using UnityEngine;

[RequireComponent (typeof (Collider2D))]
public class Door : Singleton<Door> {

    [SerializeField]
    private GameObject[] openDoorPieces;
    [SerializeField]
    private GameObject[] closedDoorPieces;

    private Collider2D col;
	private bool isOpen;

    protected override void Awake ()
    {
        base.Awake ();
        col = GetComponent<Collider2D> ();
    }

    private void Start ()
    {
        isOpen = false;
        UpdateDoorOpenStatus (isOpen);
    }

    private void OnTriggerEnter2D (Collider2D other)
    {
        if (other.gameObject.tag == "Player")
        {
            ScoreManager.Instance.PlayerWon ();
        }
    }

    public void UpdateDoorOpenStatus (bool isOpen)
    {
        foreach (GameObject door in openDoorPieces)
        {
            door.SetActive (isOpen);
        }
        foreach (GameObject door in closedDoorPieces)
        {
            door.SetActive (!isOpen);
        }
        col.enabled = isOpen;
    }

}

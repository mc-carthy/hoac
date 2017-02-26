using UnityEngine;
using UnityEngine.UI;

public class CoinManager : Singleton<CoinManager> {

    [SerializeField]
    private Text coinText;
    private int totalCoins;
	private int coinsCollected;

    private void Start ()
    {
        totalCoins = GameObject.FindObjectsOfType<Coin> ().Length;
        UpdateCoinText ();
    }

    public void AddToCoinCount ()
    {
        coinsCollected++;
        if (coinsCollected == totalCoins)
        {
            OpenDoor ();
        }
        UpdateCoinText ();
    }

    private void UpdateCoinText ()
    {
        coinText.text = "Coins: " + coinsCollected.ToString () + " / " + totalCoins.ToString ();
    }

    private void OpenDoor ()
    {
        Door.Instance.UpdateDoorOpenStatus (true);
    }

}

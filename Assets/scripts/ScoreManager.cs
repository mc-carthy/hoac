using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : Singleton<ScoreManager> {

	[SerializeField]
    private Text timeText;
    [SerializeField]
    private Text winText;

    private bool isGameOver;

    protected override void Awake ()
    {
        base.Awake ();
        winText.enabled = false;
    }

    private void Update ()
    {
        if (!isGameOver)
        {
            timeText.text = "Time: " + Time.timeSinceLevelLoad.ToString ("F2");
        }
    }

    public void PlayerWon ()
    {
        winText.enabled = true;
        isGameOver = true;
    }

}

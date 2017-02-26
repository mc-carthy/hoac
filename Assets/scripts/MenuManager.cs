using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour {

	public void GoToNextScene ()
    {
        SceneManager.LoadScene (SceneManager.GetActiveScene ().buildIndex + 1, LoadSceneMode.Single);
    }

    public void Quit ()
    {
        Application.Quit ();
    }

}

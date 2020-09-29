using UnityEngine;
using UnityEngine.SceneManagement;

public class MultiplayerMenu : MonoBehaviour
{
    public void Cancel()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
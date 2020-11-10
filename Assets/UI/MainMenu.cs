using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public static string discordURL = "https://discord.gg/EbmpduX";

    public void Start()
    {
        Cursor.visible = true;
    }

    public void Singleplayer()
    {
        SceneManager.LoadScene("SingleplayerMenu");
    }

    public void Multiplayer()
    {
        SceneManager.LoadScene("MultiplayerMenu");
    }

    public void Options()
    {
        SceneManager.LoadScene("Options");
    }

    public void Discord()
    {
        Application.OpenURL(discordURL);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
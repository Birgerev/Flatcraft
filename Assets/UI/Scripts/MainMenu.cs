using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public static string discordURL = "https://discord.gg/kfY6MyTFNK";
    public Text versionText;

    public GameObject singleplayerMenuPrefab;
    public GameObject multiplayerMenuPrefab;
    public GameObject optionsMenuPrefab;
    
    public void Start()
    {
        versionText.text = "Flatcraft " + VersionController.GetVersionName();
    }

    public void Singleplayer()
    {
        Instantiate(singleplayerMenuPrefab);
    }

    public void Multiplayer()
    {
        Instantiate(multiplayerMenuPrefab);
    }

    public void Options()
    {
        Instantiate(optionsMenuPrefab);
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
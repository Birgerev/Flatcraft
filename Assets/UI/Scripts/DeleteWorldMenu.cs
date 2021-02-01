using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DeleteWorldMenu : MonoBehaviour
{
    public static int selectedWorld = -1;
    public Text deleteText;
    public List<World> worlds = new List<World>();

    public void Start()
    {
        worlds = SingleplayerMenu.GetWorlds();

        deleteText.text = "'" + worlds[selectedWorld].name + "' will be lost forever! (A long time!)";
    }

    public void Cancel()
    {
        SceneManager.LoadScene("SingleplayerMenu");
    }

    public void Delete()
    {
        worlds[selectedWorld].Delete();
        SceneManager.LoadScene("SingleplayerMenu");
    }
}
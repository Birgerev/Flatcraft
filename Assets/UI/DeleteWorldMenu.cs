using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DeleteWorldMenu : MonoBehaviour
{
    public static int selectedWorld = -1;
    public List<World> worlds = new List<World>();
    public Text deleteText;

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

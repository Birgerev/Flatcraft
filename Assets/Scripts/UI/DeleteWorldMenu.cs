﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DeleteWorldMenu : MonoBehaviour
{
    public static int selectedWorld = -1;
    
    public GameObject singleplayerMenuPrefab;
    public Text deleteText;
    public List<World> worlds = new List<World>();

    public void Start()
    {
        worlds = SingleplayerMenu.GetWorlds();

        deleteText.text = "'" + worlds[selectedWorld].name + "' will be lost forever! (A long time!)";
    }

    public void Cancel()
    {
        Instantiate(singleplayerMenuPrefab);
        Destroy(gameObject);
    }

    public void Delete()
    {
        worlds[selectedWorld].Delete();
        
        Instantiate(singleplayerMenuPrefab);
        Destroy(gameObject);
    }
}
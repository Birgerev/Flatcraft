using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SingleplayerMenu : MonoBehaviour
{
    public Transform list;
    public int selectedWorld = -1;
    public GameObject worldPrefab;
    public List<World> worlds = new List<World>();
    public Button playButton;
    public Button deleteButton;


    private void Start()
    {
        LoadWorlds();
    }

    private void Update()
    {
        playButton.interactable = (selectedWorld != -1);
        deleteButton.interactable = (selectedWorld != -1);
    }

    public void LoadWorlds()
    {
        worlds.Clear();
        worlds = GetWorlds();

        foreach (var world in worlds)
        {
            var obj = Instantiate(worldPrefab, list, false);
        }
    }

    public static List<World> GetWorlds()
    {
        return World.loadWorlds();
    }

    public void Cancel()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void Play()
    {
        WorldManager.world = worlds[selectedWorld];
        SceneManager.LoadScene("Game");
        GameNetworkManager.isHost = true;
    }

    public void Delete()
    {
        DeleteWorldMenu.selectedWorld = selectedWorld;
        SceneManager.LoadScene("DeleteWorld");
    }

    public void Create()
    {
        SceneManager.LoadScene("CreateWorldMenu");
    }
}
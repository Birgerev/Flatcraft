using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
        playButton.interactable = selectedWorld != -1;
        deleteButton.interactable = selectedWorld != -1;
    }

    public void LoadWorlds()
    {
        worlds.Clear();
        worlds = GetWorlds();

        foreach (World world in worlds)
        {
            GameObject obj = Instantiate(worldPrefab, list, false);
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
        Sound.PlayLocal(new Location(), "menu/click", 0, SoundType.Menu, 1f, 100000f, false);
        WorldManager.world = worlds[selectedWorld];
        SceneManager.LoadScene("Game");
        GameNetworkManager.isHost = true;
        LoadingMenu.Create(LoadingMenuType.LoadWorld);
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
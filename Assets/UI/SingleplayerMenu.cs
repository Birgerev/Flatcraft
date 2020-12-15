using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SingleplayerMenu : MonoBehaviour
{
    public Transform list;
    public int selectedWorld = -1;
    public GameObject worldPrefab;
    public List<World> worlds = new List<World>();


    private void Start()
    {
        LoadWorlds();
        Cursor.visible = true;
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
        SceneManager.LoadScene("Loading");
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
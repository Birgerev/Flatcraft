using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SingleplayerMenu : MonoBehaviour
{
    public List<World> worlds = new List<World>();
    public Transform list;
    public GameObject worldPrefab;
    public int selectedWorld = -1;


    private void Start()
    {
        LoadWorlds();
    }

    public void LoadWorlds()
    {
        worlds.Clear();

        worlds = GetWorlds();

        foreach (World world in worlds)
        {
            GameObject obj = Instantiate(worldPrefab);

            obj.transform.SetParent(list);
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

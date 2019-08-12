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

        worlds = World.loadWorlds();

        foreach (World world in worlds)
        {
            GameObject obj = Instantiate(worldPrefab);

            obj.transform.SetParent(list);
        }
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
        worlds[selectedWorld].Delete();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Create()
    {
        SceneManager.LoadScene("CreateWorldMenu");
    }
}

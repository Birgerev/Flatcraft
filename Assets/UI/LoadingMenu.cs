using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingMenu : MonoBehaviour
{
    WorldManager worldManager;

    public Text loadingState;
    public Image loadingBar;

    // Start is called before the first frame update
    void Start()
    {
        SceneManager.LoadSceneAsync("Game", LoadSceneMode.Additive);
    }

    // Update is called once per frame
    void Update()
    {
        if (worldManager == null)
            if (WorldManager.instance != null)
                worldManager = WorldManager.instance;
            else return;
        
        loadingState.text = worldManager.loadingState;
        loadingBar.fillAmount = worldManager.loadingProgress;

        if (worldManager.loadingProgress >= 1)
        {
            SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName("Loading"));
        }
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingMenu : MonoBehaviour
{
    public Image loadingBar;

    public Text loadingState;
    private WorldManager worldManager;

    // Start is called before the first frame update
    private void Start()
    {
        SceneManager.LoadSceneAsync("Game", LoadSceneMode.Additive);
    }

    // Update is called once per frame
    private void Update()
    {
        if (worldManager == null)
            if (WorldManager.instance != null)
                worldManager = WorldManager.instance;
            else return;

        loadingState.text = worldManager.loadingState;
        loadingBar.fillAmount = worldManager.loadingProgress;

        if (worldManager.loadingProgress >= 1) SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName("Loading"));
    }
}
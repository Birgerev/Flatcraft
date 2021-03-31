using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingMenu : MonoBehaviour
{
    public static LoadingMenu instance;
    
    public Image loadingBar;
    public Text loadingStateText;
    
    public string loadingState;
    public float loadingProgress;

    // Start is called before the first frame update
    private void Start()
    {
        SceneManager.LoadSceneAsync("Game", LoadSceneMode.Additive);
        instance = this;
    }

    // Update is called once per frame
    private void Update()
    {
        loadingStateText.text = loadingState;
        loadingBar.fillAmount = loadingProgress;

        if (loadingProgress >= 1) 
            Destroy(gameObject);
    }

    public static void Create()
    {
        GameObject prefab = Resources.Load<GameObject>("Prefabs/LoadingMenu");
        GameObject obj = Instantiate(prefab);
    }
}
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static bool active;

    // Start is called before the first frame update
    private void Start()
    {
        active = false;
    }

    // Update is called once per frame
    private void Update()
    {
        if (WorldManager.instance.loadingProgress != 1)
        {
            Cursor.visible = false;
            active = false;
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.visible = active;
            active = !active;
            Time.timeScale = active ? 0 : 1;
        }

        GetComponent<CanvasGroup>().alpha = active ? 1 : 0;
        GetComponent<CanvasGroup>().interactable = active;
        GetComponent<CanvasGroup>().blocksRaycasts = active;
    }

    public void BackToGame()
    {
        Time.timeScale = 1;
        active = false;
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
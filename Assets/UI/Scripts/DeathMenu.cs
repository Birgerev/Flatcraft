using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathMenu : MonoBehaviour
{
    public static bool active;

    // Start is called before the first frame update
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {

        GetComponent<CanvasGroup>().alpha = active ? 1 : 0;
        GetComponent<CanvasGroup>().interactable = active;
        GetComponent<CanvasGroup>().blocksRaycasts = active;
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("MainMenu");
        active = false;
    }

    public void Respawn()
    {
        WorldManager.instance.Spawn();
        active = false;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class DeathMenu : MonoBehaviour
{
    public static bool active = false;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        GetComponent<CanvasGroup>().alpha = (active) ? 1 : 0;
        GetComponent<CanvasGroup>().interactable = (active);
        GetComponent<CanvasGroup>().blocksRaycasts = (active);
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

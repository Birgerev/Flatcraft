using System;
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
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SetMenuActive(!active);
        }
    }

    public void EnterMenu()
    {
        SetMenuActive(true);
    }

    public void BackToGame()
    {
        SetMenuActive(false);
    }

    public void SetMenuActive(bool setActive)
    {
        active = setActive;
        
        Cursor.visible = active;
        Time.timeScale = active ? 0 : 1;

        GetComponent<CanvasGroup>().alpha = active ? 1 : 0;
        GetComponent<CanvasGroup>().interactable = active;
        GetComponent<CanvasGroup>().blocksRaycasts = active;
    }

    public void BackToMainMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("MainMenu");
    }
}
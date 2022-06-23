using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F11))
            ToggleFullscreen();
    }


    public static void ToggleFullscreen()
    {
        if(IsFullscreen())
            Screen.SetResolution(1000, 580, FullScreenMode.Windowed);
        else
            Screen.SetResolution(Display.main.systemWidth, Display.main.systemHeight,
                FullScreenMode.ExclusiveFullScreen);
    }
    
    public static bool IsFullscreen()
    {
        return (Screen.fullScreenMode == FullScreenMode.ExclusiveFullScreen);
    }
}

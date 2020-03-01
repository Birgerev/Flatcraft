using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIButtonSound : MonoBehaviour
{
    public void Play()
    {
        Sound.Play(new Location(), "menu_click", SoundType.Menu, 1);
        print("click");
    }

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(Play);
    }
}

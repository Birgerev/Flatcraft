using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIButtonSound : MonoBehaviour
{
    public void Play()
    {
        Sound.Play(new Location(), "menu/click", SoundType.Menu, 0.8f, 1.2f);
    }

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(Play);
    }
}

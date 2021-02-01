using UnityEngine;
using UnityEngine.UI;

public class UIButtonSound : MonoBehaviour
{
    public void Play()
    {
        Sound.Play(new Location(), "menu/click", SoundType.Menu, 1f, 1f);
    }

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(Play);
    }
}
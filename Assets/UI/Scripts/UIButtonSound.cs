using UnityEngine;
using UnityEngine.UI;

public class UIButtonSound : MonoBehaviour
{
    public void Play()
    {
        Sound.PlayLocal(new Location(), "menu/click", 0, SoundType.Menu, 1f, 100000f, false);
    }

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(Play);
    }
}
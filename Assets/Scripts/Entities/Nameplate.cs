using UnityEngine;
using UnityEngine.UI;

public class Nameplate : MonoBehaviour
{
    public string text = "";
    public Text textComponent;
    public CanvasGroup canvasGroup;

    private void Update()
    {
        textComponent.text = text;
        canvasGroup.alpha = text.Equals("") ? 0 : 1;
    }
}
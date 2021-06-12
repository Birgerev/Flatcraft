using UnityEngine;
using UnityEngine.UI;

public class ChatEntry : MonoBehaviour
{
    private const float beginFadeAge = 4;
    private const float fadeTime = 1;

    public string message;
    public Text text;
    public CanvasGroup canvasGroup;

    private float age;

    // Start is called before the first frame update
    private void Start()
    {
        text.text = message;
    }

    // Update is called once per frame
    private void Update()
    {
        age += Time.deltaTime;

        if (ChatMenu.instance.open)
        {
            canvasGroup.alpha = 1;
        }
        else
        {
            if (age >= beginFadeAge)
            {
                float fadeTimePassed = age - beginFadeAge;
                float fadeFactor = fadeTimePassed / fadeTime;

                canvasGroup.alpha = Mathf.Clamp(1 - fadeFactor, 0, 1);
            }
        }
    }
}
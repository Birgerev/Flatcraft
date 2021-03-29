using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActionBar : MonoBehaviour
{
    const float BeginFadeAge = 2f;
    const float FadeTime = 0.5f;
    
    public static string message = "";
    public Text text;
    public CanvasGroup canvasGroup;

    private float age;
    private string lastFrameMessage = "";
    
    // Update is called once per frame
    void Update()
    {
        age += Time.deltaTime;
        text.text = message;

        if (!message.Equals(lastFrameMessage) && !message.Equals(""))
            age = 0;

        if (age >= BeginFadeAge)
        {
            float fadeTimePassed = age - BeginFadeAge;
            float fadeFactor = fadeTimePassed / FadeTime;

            canvasGroup.alpha = Mathf.Clamp(1 - fadeFactor, 0, 1);
        }
        else
        {
            canvasGroup.alpha = 1;
        }

        if (age >= BeginFadeAge + FadeTime)
            message = "";

        lastFrameMessage = message;
    }
}

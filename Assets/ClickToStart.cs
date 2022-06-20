using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClickToStart : MonoBehaviour
{
    public float blinkFrequency;
    public CanvasGroup buttonsGroup;
    public Text text;

    private CanvasGroup thisGroup;

    // Start is called before the first frame update
    void Start()
    {
        thisGroup = GetComponent<CanvasGroup>();
        StartCoroutine(TextBlinkLoop());
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.anyKeyDown)
        {
            Sound.PlayLocal(new Location(), "menu/click", 0, SoundType.Menu, 1f, 100000f, false);

            buttonsGroup.interactable = true;
            buttonsGroup.blocksRaycasts = true;
            buttonsGroup.alpha = 1;

            Destroy(gameObject);
        }
    }

    IEnumerator TextBlinkLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(blinkFrequency);
            Color curColor = text.color;
            curColor.a = (curColor.a == 0) ? 1 : 0;
            text.color = curColor;
        }
    }
}

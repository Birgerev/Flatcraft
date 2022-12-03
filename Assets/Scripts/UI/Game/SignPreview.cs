using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SignPreview : MonoBehaviour
{
    public Text[] uiTexts;
    public string[] lines;
    
    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < uiTexts.Length; i++)
        {
            uiTexts[i].text = lines[i];
        }
    }
}

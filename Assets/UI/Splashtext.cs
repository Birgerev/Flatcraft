using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System.IO;

public class Splashtext : MonoBehaviour
{
    public Text splashtextElement;
    public string splashtext;

    // Start is called before the first frame update
    void Start()
    {
        string[] splashtexts = File.ReadAllLines(Application.dataPath + "\\Resources\\splashtexts.txt");
        splashtext = splashtexts[Random.Range(0, splashtexts.Length)];

        UpdateText();
    }

    // Update text element
    void UpdateText()
    {
        splashtextElement.text = splashtext;
    }
}

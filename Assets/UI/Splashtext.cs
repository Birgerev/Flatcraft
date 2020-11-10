using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System.IO;
using UnityEngine.Assertions.Comparers;
using Random = UnityEngine.Random;

public class Splashtext : MonoBehaviour
{
    public string splashtext;
    public float sizeBounceSpeed;
    public float sizeMultiplier;

    private float sizeIndex;

    // Start is called before the first frame update
    void Start()
    {
        string[] splashtexts = File.ReadAllLines(Application.dataPath + "\\Resources\\splashtexts.txt");
        splashtext = splashtexts[Random.Range(0, splashtexts.Length)];

        UpdateText();
    }

    private void Update()
    {
        sizeIndex += Time.deltaTime * sizeBounceSpeed;
        float size = 1 - Math.Abs(((float)Math.Cos(sizeIndex)) * sizeMultiplier);
        transform.localScale = new Vector3(size, size, 1);
    }

    // Update text element
    void UpdateText()
    {
        GetComponent<Text>().text = splashtext;
    }
}

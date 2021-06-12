using System;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Splashtext : MonoBehaviour
{
    public string splashtext;
    public float sizeBounceSpeed;
    public float sizeMultiplier;

    private float sizeIndex;

    // Start is called before the first frame update
    private void Start()
    {
        string[] splashtexts = (Resources.Load("splashtexts") as TextAsset).text.Split('\n');
        splashtext = splashtexts[Random.Range(0, splashtexts.Length)];

        UpdateText();
    }

    private void Update()
    {
        sizeIndex += Time.deltaTime * sizeBounceSpeed;
        float size = 1 - Math.Abs((float) Math.Cos(sizeIndex) * sizeMultiplier);
        transform.localScale = new Vector3(size, size, 1);
    }

    // Update text element
    private void UpdateText()
    {
        GetComponent<Text>().text = splashtext;
    }
}
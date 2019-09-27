using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class BlockLight : MonoBehaviour
{
    public int glowingLevel = 0;
    public float flickerLevel = 0;
    public float intensity = 1;
    private int flickersInASecond = 2;
    public Color color;

    private float flickerValue = 0;

    // Start is called before the first frame update
    void Awake()
    {
        Update();
        StartCoroutine(Flicker());
    }

    // Update is called once per frame
    void Update()
    {
        GetComponent<Light2D>().lightType = Light2D.LightType.Point;
        GetComponent<Light2D>().pointLightInnerRadius = (glowingLevel / 10) + flickerValue;
        GetComponent<Light2D>().pointLightOuterRadius = glowingLevel;
        GetComponent<Light2D>().color = color;
        GetComponent<Light2D>().intensity = intensity;
        transform.GetChild(0).GetComponent<Light2D>().lightType = Light2D.LightType.Point;
        transform.GetChild(0).GetComponent<Light2D>().pointLightInnerRadius = (glowingLevel / 10) + flickerValue;
        transform.GetChild(0).GetComponent<Light2D>().pointLightOuterRadius = glowingLevel;
        transform.GetChild(0).GetComponent<Light2D>().color = color;
        transform.GetChild(0).GetComponent<Light2D>().intensity = intensity;
    }

    IEnumerator Flicker()
    {
        while (true)
        {
            flickerValue = (flickerValue == 0) ? flickerLevel : 0;
            yield return new WaitForSecondsRealtime(1 / flickersInASecond);
        }
    }
}

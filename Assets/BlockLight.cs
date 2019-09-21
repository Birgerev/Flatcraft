using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class BlockLight : MonoBehaviour
{
    public int glowingLevel = 0;
    public float flickerLevel = 0;
    public int flickersInASecond = 2;
    public Color color;

    private float flickerValue = 0;
    private float last_flicker_time;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (GetComponent<Light2D>() == null)
            gameObject.AddComponent<Light2D>();
        GetComponent<Light2D>().lightType = Light2D.LightType.Point;
        GetComponent<Light2D>().pointLightInnerRadius = (glowingLevel / 10) + flickerValue;
        GetComponent<Light2D>().pointLightOuterRadius = glowingLevel;
        GetComponent<Light2D>().color = color;

        if (Time.time > last_flicker_time + (1 / flickersInASecond))
        {
            Flicker();
            last_flicker_time = Time.time;
        }
    }

    public void Flicker()
    {
        flickerValue = (flickerValue == 0) ? flickerLevel : 0;
    }
}

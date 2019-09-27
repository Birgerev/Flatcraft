using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sky : MonoBehaviour
{
    public Color dayColor;
    public Color nightColor;
    public Color transitionColor;

    private float timeOfDay;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        timeOfDay = (WorldManager.world.time % WorldManager.dayLength) / WorldManager.dayLength;

        moveSky();
        skyColor();
        lightLevels();
    }

    void moveSky()
    {
        if (Player.localInstance == null)
            return;
        transform.position = new Vector3(Player.localInstance.transform.position.x, transform.position.y);
    }

    void lightLevels()
    {
        float intensity = (timeOfDay < 0.5f) ? 1 : 0.1f;

        Sunlight.instance.intensity = intensity;
    }

    void skyColor()
    {
        Color color = Color.white;

        if (timeOfDay < 1)
        {
            float stateValue = (timeOfDay - 0.95f) / 0.05f;
            Color colorA;
            Color colorB;
            if (stateValue < 0.5f)
            {
                colorA = nightColor;
                colorB = transitionColor;
            }
            else
            {
                colorA = transitionColor;
                colorB = dayColor;
            }
            
            color = Color.Lerp(colorA, colorB, stateValue);
        }
        if (timeOfDay < 0.95f)
        {
            color = nightColor;
        }
        if (timeOfDay <= 0.55f)
        {
            float stateValue = (timeOfDay - 0.50f)/0.05f;
            Color colorA;
            Color colorB;
            if (stateValue < 0.5f)
            {
                colorA = dayColor;
                colorB = transitionColor;
            }
            else
            {
                colorA = transitionColor;
                colorB = nightColor;
            }
            
            color = Color.Lerp(colorA, colorB, stateValue);
        }
        if (timeOfDay < 0.50f)
        {
            color = dayColor;
        }

        GetComponent<SpriteRenderer>().color = color;
    }
}

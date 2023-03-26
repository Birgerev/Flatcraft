using System;
using UnityEngine;

public class LightObject : MonoBehaviour
{
    public int lightLevelDeduct;
    public bool staticObject;
    private Vector3 position;

    public Vector3 GetPosition()
    {
        if (staticObject)
        {
            if (position == Vector3.zero)
                position = transform.position;

            return position;
        }

        return position = transform.position;
    }

    public Location GetLocation()
    {
        return Location.LocationByPosition(GetPosition());
    }
    
    public void UpdateLightLevel(int lightLevel)
    {
        //Enforce minimum light level in nether
        if (GetLocation().dimension == Dimension.Nether)
            lightLevel = Mathf.Clamp(lightLevel, LightManager.NetherLightLevel, int.MaxValue);
        
        //Deduct predefined light level
        lightLevel -= lightLevelDeduct;
        //Ensure light level is in correct range
        lightLevel = Mathf.Clamp(lightLevel, 0, LightManager.MaxLightLevel);

        //Calculate 01 value and then decide resulting color
        float lightLevel01 = lightLevel / (float) LightManager.MaxLightLevel;

        //Assign new color
        GetComponent<SpriteRenderer>().material.SetFloat("_BrightnessValue", lightLevel01);
    }

    public void RequestLightUpdate()
    {
        LightManager.UpdateLightObject(this);
    }
}
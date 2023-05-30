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
    
    public void UpdateLightLevel(LightValues lightValues)
    {
        
        //Deduct predefined light level
        lightValues.lightLevel -= lightLevelDeduct;
        
        //Ensure light level is within valid range
        int minLightLevel = 0;
        
        //Enforce minimum light level in nether
        if (GetLocation().dimension == Dimension.Nether)
            minLightLevel = LightManager.NetherLightLevel;
        
        lightValues.lightLevel = Mathf.Clamp(lightValues.lightLevel, minLightLevel, LightManager.MaxLightLevel);

        //Calculate 01 value and then decide resulting color
        float lightLevel01 = lightValues.lightLevel / (float) LightManager.MaxLightLevel;
        

        //Assign new color
        Color brightnessColor = lightValues.sourceColor * lightLevel01;//TODO replace white with light color
        
        GetComponent<SpriteRenderer>().material.SetColor("_LightColor", brightnessColor);
        GetComponent<SpriteRenderer>().material.SetFloat("_LightFlicker", lightValues.flicker?1:0);
        
        //TODO set flicker
    }

    public void RequestLightUpdate()
    {
        LightManager.UpdateLightObjectWithUnknownSourcesAsync(this);
    }
}
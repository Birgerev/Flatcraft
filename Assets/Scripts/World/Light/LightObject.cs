using System;
using UnityEngine;

public class LightObject : MonoBehaviour
{
    public int lightLevelDeduct;
    public bool staticObject;
    private Location? _cachedLocation;

    public Location GetLocation()
    {
        if (staticObject)
            return Location.LocationByPosition(transform.position);

        if (!_cachedLocation.HasValue)
            _cachedLocation = Location.LocationByPosition(transform.position);

        return _cachedLocation.Value;
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
        LightManager.UpdateLightObject(this);
    }
}
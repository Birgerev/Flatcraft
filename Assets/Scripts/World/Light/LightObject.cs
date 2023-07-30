using System;
using UnityEngine;

public class LightObject : MonoBehaviour
{
    public int lightLevelDeduct;
    public bool staticObject;
    
    private Location _cachedLocation;

    private void Start()
    {
        CacheLocation();
    }

    public void CacheLocation()
    {
        if (staticObject)
            _cachedLocation = Location.LocationByPosition(transform.position);
    }
    
    public Location GetLocation()
    {
        //If object location can't be cached, just return it
        if (!staticObject)
            return Location.LocationByPosition(transform.position);
        
        //return cached loc
        return _cachedLocation;
    }
    
    public void UpdateLightLevel(LightValues lightValues)
    {
        //Enforce minimum light level in nether
        if (GetLocation().dimension == Dimension.Nether)
            lightValues.lightLevel = Mathf.Max(lightValues.lightLevel , LightManager.NetherLightLevel);
        
        //Deduct predefined light level, after dimension light so backgrounds etc can be darker
        lightValues.lightLevel -= lightLevelDeduct;
        
        //Clamp light level between min and max levels
        lightValues.lightLevel = Mathf.Clamp(lightValues.lightLevel, 0, LightManager.MaxLightLevel);

        //Calculate 01 value
        float lightLevel01 = lightValues.lightLevel / (float) LightManager.MaxLightLevel;
        

        //Resulting color is light source color multiplied by current brightness
        Color brightnessColor = lightValues.sourceColor * lightLevel01;
        
        GetComponent<SpriteRenderer>().material.SetColor("_LightColor", brightnessColor);
        GetComponent<SpriteRenderer>().material.SetFloat("_LightFlicker", lightValues.flicker?1:0);
        
        //TODO set flicker
    }

    public void RequestLightUpdate()
    {
        LightManager.UpdateLightObject(this);
    }
}
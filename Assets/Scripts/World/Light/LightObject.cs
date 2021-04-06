using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Mathematics;

public class LightObject : MonoBehaviour
{
    public int lightLevelDeduct;

    public void UpdateLightLevel(int lightLevel)
    {
        lightLevel -= lightLevelDeduct;
        lightLevel = Mathf.Clamp(lightLevel, 0, LightManager.maxLightLevel);

        Player player = Player.localEntity;
        if (player == null)
            return;
        
        if (GetLocation().dimension == Dimension.Nether)
            lightLevel = Mathf.Clamp(lightLevel, LightManager.netherLightLevel, Int32.MaxValue);
        
        float lightLevelFactor = (float)lightLevel / (float)LightManager.maxLightLevel;
        Color color = new Color(lightLevelFactor, lightLevelFactor, lightLevelFactor, 1);

        GetComponent<SpriteRenderer>().color = color;
    }
    
    public Location GetLocation()
    {
        return Location.LocationByPosition(transform.position);
    }
}

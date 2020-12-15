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

        var lightLevelFactor = (float)lightLevel / (float)LightManager.maxLightLevel;
        var color = new Color(lightLevelFactor, lightLevelFactor, lightLevelFactor, 1);

        GetComponent<SpriteRenderer>().color = color;
    }
}

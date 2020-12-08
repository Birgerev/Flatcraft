using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Mathematics;

public class LightObject : MonoBehaviour
{
    public void UpdateLightLevel(int lightLevel)
    {
        var lightLevelFactor = (float)lightLevel / (float)LightManager.maxLightLevel;

        var color = new Color(lightLevelFactor, lightLevelFactor, lightLevelFactor, 1);

        GetComponent<SpriteRenderer>().color = color;
    }
}

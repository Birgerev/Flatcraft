using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class LightSource : MonoBehaviour
{
    public int lightLevel;

    public void UpdateLightLevel(int value)
    {
        lightLevel = value;

        int2 location = new int2((int)transform.position.x, (int)transform.position.y);
        LightManager.UpdateLightInArea(location - new int2(15, 15), location + new int2(15, 15));
    }
    private void OnDestroy()
    {
        int2 location = new int2((int)transform.position.x, (int)transform.position.y);
        LightManager.UpdateLightInArea(location - new int2(15, 15), location + new int2(15, 15));
    }
}

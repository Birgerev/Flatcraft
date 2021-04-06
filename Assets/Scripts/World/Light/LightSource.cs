using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class LightSource : MonoBehaviour
{
    public int lightLevel;

    public void UpdateLight()
    {
        Location loc = GetLocation();
        LightManager.UpdateLightInArea(loc + new Location(-15, -15), loc + new Location(15, 15));
    }
    
    public void UpdateLightLevel(int value, bool updateLight)
    {
        lightLevel = value;

        bool chunkLoaded = new ChunkPosition(GetLocation()).IsChunkLoaded();
        
        if (updateLight && chunkLoaded)
            UpdateLight();
    }
    
    public Location GetLocation()
    {
        return Location.LocationByPosition(transform.position);
    }
}

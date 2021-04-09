using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class LightSource : MonoBehaviour
{
    public int lightLevel;
    public Vector3 position;
    public Location location;

    public void Start()
    {
        position = transform.position;
        location = Location.LocationByPosition(position);
    }


    public void UpdateLight()
    {
        LightManager.UpdateLightInArea(location + new Location(-15, -15), location + new Location(15, 15));
    }
    
    public void UpdateLightLevel(int value, bool updateLight)
    {
        lightLevel = value;

        bool chunkLoaded = new ChunkPosition(location).IsChunkLoaded();
        
        if (updateLight && chunkLoaded)
            UpdateLight();
    }
}

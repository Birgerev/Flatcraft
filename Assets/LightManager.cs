using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class LightManager : MonoBehaviour
{
    public static int maxLightLevel = 15;
    public static LightManager instance;

    public GameObject lightSourcePrefab;
    public GameObject sunlightSourcePrefab;
    public Dictionary<int, SunlightSource> sunlightSources = new Dictionary<int, SunlightSource>();
    
    void Start()
    {
        instance = this;
    }

    public static bool DoesBlockInfluenceSunlight(int2 location)
    {
        if (instance.sunlightSources.ContainsKey(location.x))
            if (location.y >= instance.sunlightSources[location.x].transform.position.y)
                return true;

        return false;
    }

    public static void UpdateSunlightInColumn(int x)
    {
        if (instance.sunlightSources.ContainsKey(x))
        {
            SunlightSource oldSunlightSource = instance.sunlightSources[x];
            instance.sunlightSources.Remove(x);
            Destroy(oldSunlightSource.gameObject);
        }

        Block topmostBlock = Chunk.GetTopmostBlock(x, Player.localInstance.Location.dimension, false);
        GameObject newSunlightSource = Instantiate(instance.sunlightSourcePrefab, topmostBlock.transform.position, Quaternion.identity);

        instance.sunlightSources.Add(x, newSunlightSource.GetComponent<SunlightSource>());
    }

    public static void UpdateBlockLight(Location block)
    {
        UpdateLightInArea(new int2(block.x, block.y), new int2(block.x, block.y));
    }
    
    public static void UpdateChunkLight(ChunkPosition chunk)
    {
        UpdateLightInArea(new int2(chunk.worldX, 0), new int2(chunk.worldX + Chunk.Width, Chunk.Height));
    }
    
    public static void UpdateLightInArea(int2 min, int2 max)
    {
        List<LightObject> lightObjects = GetLightObjectsForArea(min, max);
        List<LightSource> lightSources = GetLightSourcesForArea(min - new int2(maxLightLevel, maxLightLevel), max + new int2(maxLightLevel, maxLightLevel));
        
        foreach(LightObject lightObject in lightObjects)
        {
            UpdateLight(lightObject, lightSources);
        }
    }

    public static void UpdateLightObject(LightObject lightObj)
    {
        Vector2 pos = lightObj.transform.position;
        int2 loc = new int2((int)pos.x, (int)pos.y);
        List<LightSource> lightSources = GetLightSourcesForArea(loc - new int2(maxLightLevel, maxLightLevel), loc + new int2(maxLightLevel, maxLightLevel));

        UpdateLight(lightObj, lightSources);
    }

    public static void UpdateLight(LightObject lightObject, List<LightSource> possibleLightSources)
    {
        var objectLoc = lightObject.transform.position;
        var brightestRecordedLightLevel = 0;

        foreach (LightSource source in possibleLightSources)
        {
            var sourceLoc = source.transform.position;
            var sourceBrightness = source.lightLevel;
            var objectDistance = math.distance(sourceLoc, objectLoc);
            var objectBrightness = sourceBrightness - (int) objectDistance;
            
            if (objectBrightness > brightestRecordedLightLevel)
            {
                brightestRecordedLightLevel = objectBrightness;
            }
        }
        
        lightObject.UpdateLightLevel(brightestRecordedLightLevel);
    }

    public static List<LightObject> GetLightObjectsForArea(int2 boundingBoxMin, int2 boundingBoxMax)
    {
        
        Collider2D[] lightObjectColliders = Physics2D.OverlapAreaAll(
            new Vector2(boundingBoxMin.x, boundingBoxMin.y),
            new Vector2(boundingBoxMax.x, boundingBoxMax.y));
        List<LightObject> lightObjects = new List<LightObject>();
        foreach (Collider2D lightObjectCollider in lightObjectColliders)
        {
            LightObject lightObject = lightObjectCollider.GetComponent<LightObject>();
            
            if(lightObject != null)
                lightObjects.Add(lightObject);
        }
        
        return lightObjects;
    }

    public static List<LightSource> GetLightSourcesForArea(int2 boundingBoxMin, int2 boundingBoxMax)
    {
        Collider2D[] lightSourceColliders = Physics2D.OverlapAreaAll(
            new Vector2(boundingBoxMin.x, boundingBoxMin.y),
            new Vector2(boundingBoxMax.x, boundingBoxMax.y),
            LayerMask.GetMask("LightSource"));
        List<LightSource> lightSources = new List<LightSource>();
        foreach (Collider2D lightSourceCollider in lightSourceColliders)
        {
            lightSources.Add(lightSourceCollider.GetComponent<LightSource>());
        }

        return lightSources;
    }
}

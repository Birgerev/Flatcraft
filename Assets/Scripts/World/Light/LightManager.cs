using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Unity.Burst;

[BurstCompile]
public class LightManager : MonoBehaviour
{
    public static int maxLightLevel = 15;
    public static int netherLightLevel = 8;
    public static LightManager instance;

    public bool doLight = true;
    private bool doLightLastFrame = true;
    public GameObject lightSourcePrefab;
    public GameObject sunlightSourcePrefab;
    public Dictionary<int, SunlightSource> sunlightSources = new Dictionary<int, SunlightSource>();
    
    void Start()
    {
        instance = this;
    }

    void Update()
    {
        if (doLight != doLightLastFrame)
        {
            foreach (var chunkPos in WorldManager.instance.chunks.Keys)
            {
                UpdateChunkLight(chunkPos);
            }

            doLightLastFrame = doLight;
        }
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

        //Dont create sunlight sources if player is in the nether
        if (Player.localInstance.Location.dimension == Dimension.Nether)
            return;
        
        Block topmostBlock = Chunk.GetTopmostBlock(x, Player.localInstance.Location.dimension, false);
        GameObject newSunlightSource = Instantiate(instance.sunlightSourcePrefab, topmostBlock.transform.position, Quaternion.identity);

        instance.sunlightSources.Add(x, newSunlightSource.GetComponent<SunlightSource>());
    }

    public static void DestroySource(GameObject source)
    {
        int2 location = new int2((int)source.transform.position.x, (int)source.transform.position.y);
        LightManager.UpdateLightInArea(location - new int2(15, 15), location + new int2(15, 15));
        Destroy(source);
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

    public static void UpdateLightForSources(List<LightSource> sources)
    {
        HashSet<LightObject> lightObjects = new HashSet<LightObject>();

        foreach (LightSource source in sources)
        {
            Vector3 sourcePos = source.transform.position;
            lightObjects.UnionWith(GetLightObjectsForArea(
                new int2((int)sourcePos.x - maxLightLevel, (int)sourcePos.y - maxLightLevel),
                new int2((int)sourcePos.x + maxLightLevel, (int)sourcePos.y + maxLightLevel)));
        }
        
        foreach(LightObject lightObject in lightObjects)
        {
            
            UpdateLight(lightObject, sources);
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
        Vector3 objectLoc = lightObject.transform.position;
        int brightestRecordedLightLevel = 0;

        if (LightManager.instance.doLight)
            foreach (LightSource source in possibleLightSources)
            {
                
                Vector3 sourceLoc = source.transform.position;
                float objectDistance = Vector3.Distance(sourceLoc, objectLoc);
                if(objectDistance > maxLightLevel)
                    continue;
                
                int sourceBrightness = source.lightLevel;
                int objectBrightness = sourceBrightness - (int) objectDistance;
                
                if (objectBrightness > brightestRecordedLightLevel)
                {
                    brightestRecordedLightLevel = objectBrightness;
                }
            }
        else
            brightestRecordedLightLevel = 15;
        
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

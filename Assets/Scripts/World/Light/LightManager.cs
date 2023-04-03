using System;
using System.Collections.Generic;
using Unity.Burst;
using UnityEngine;

[BurstCompile]
public class LightManager : MonoBehaviour
{
    public const int MaxLightLevel = 15;
    public const int NightLightLevel = 5;
    public const int NetherLightLevel = 7;
    
    public static LightManager Instance;
    public static bool DoLight = true;

    public GameObject lightSourcePrefab;
    public GameObject sunlightSourcePrefab;
    public Dictionary<LightColumn, SunlightSource> sunlightSources = new Dictionary<LightColumn, SunlightSource>();

    private void Start()
    {
        Instance = this;
    }

    #region Area Light Handling
    
    public static void UpdateAllLight()
    {
        foreach (object chunkPos in WorldManager.instance.chunks.Keys)
            UpdateChunkLight((ChunkPosition) chunkPos);
    }
    

    public static void UpdateChunkLight(ChunkPosition chunk)
    {
        UpdateLightInArea(new Location(chunk.worldX, 0, chunk.dimension),
            new Location(chunk.worldX + Chunk.Width, Chunk.Height, chunk.dimension));
    }

    public static void UpdateLightInArea(Location min, Location max)
    {
        List<LightObject> lightObjects = GetLightObjectsForArea(min, max);
        List<LightSource> lightSources = GetLightSourcesForArea(min + new Location(-MaxLightLevel, -MaxLightLevel),
            max + new Location(MaxLightLevel, MaxLightLevel));

        foreach (LightObject lightObject in lightObjects)
            UpdateLightObjectWithSpecificSources(lightObject, lightSources);
    }

    private static List<LightObject> GetLightObjectsForArea(Location boundingBoxMin, Location boundingBoxMax)
    {
        Collider2D[] lightObjectColliders = Physics2D.OverlapAreaAll(boundingBoxMin.GetPosition(),
            boundingBoxMax.GetPosition());
        List<LightObject> lightObjects = new List<LightObject>();

        foreach (Collider2D lightObjectCollider in lightObjectColliders)
        {
            LightObject lightObject = lightObjectCollider.GetComponent<LightObject>();

            if (lightObject != null)
                lightObjects.Add(lightObject);
        }

        return lightObjects;
    }

    private static List<LightSource> GetLightSourcesForArea(Location boundingBoxMin, Location boundingBoxMax)
    {
        Collider2D[] lightSourceColliders = Physics2D.OverlapAreaAll(boundingBoxMin.GetPosition(),
            boundingBoxMax.GetPosition(),
            LayerMask.GetMask("LightSource"));
        List<LightSource> lightSources = new List<LightSource>();

        foreach (Collider2D lightSourceCollider in lightSourceColliders)
            lightSources.Add(lightSourceCollider.GetComponent<LightSource>());

        return lightSources;
    }
    #endregion
 
    #region Sunlight
    public static bool DoesBlockInfluenceSunlight(Location loc)
    {
        LightColumn column = new LightColumn(loc.x, loc.dimension);

        if (!Instance.sunlightSources.ContainsKey(column))
            return true;
        
        if (loc.y >= Instance.sunlightSources[column].transform.position.y)
            return true;

        return false;
    }

    public static void UpdateSunlightInColumn(LightColumn column, bool updateLight)
    {
        if (Instance.sunlightSources.ContainsKey(column))
        {
            SunlightSource oldSunlightSource = Instance.sunlightSources[column];
            Instance.sunlightSources.Remove(column);

            if (updateLight)
                oldSunlightSource.lightSource.UpdateLightWithinReach();
            
            Destroy(oldSunlightSource.gameObject);
        }

        //Dont create sunlight sources if player is in the nether
        if (column.dimension == Dimension.Nether)
            return;

        Block topmostBlock = Chunk.GetTopmostBlock(column.x, column.dimension, false);

        //Return in case no block was found in column, may be the case in ex void worlds
        if (topmostBlock == null)
            return;

        SunlightSource newSunlightSource =
            SunlightSource.Create(Location.LocationByPosition(topmostBlock.transform.position));

        Instance.sunlightSources.Add(column, newSunlightSource);
        if (updateLight)
            newSunlightSource.lightSource.UpdateLightWithinReach();
    }
    #endregion

    public static void DestroySource(LightSource source)
    {
        //TODO how can order be this way?
        source.UpdateLightWithinReach();
        
        Destroy(source.gameObject);
    }

    public static int GetLightLevel(Location loc, List<LightSource> knownLightSources = null)
    {
        if (!DoLight)
            return MaxLightLevel;
        
        //If no set of light sources was supplied, automatically find them
        if(knownLightSources == null)
            knownLightSources = GetLightSourcesForArea(
                loc + new Location(-MaxLightLevel, -MaxLightLevel), 
                loc + new Location(MaxLightLevel, MaxLightLevel));
        
        //Find the brightest way to light 
        Vector3 objectPos = loc.GetPosition();
        int brightestRecordedLightLevel = 0;
        foreach (LightSource source in knownLightSources)
        {
            Vector3 sourcePos = source.position;
            float objectDistance = Vector3.Distance(sourcePos, objectPos);
            if (objectDistance > MaxLightLevel)
                continue;

            int sourceBrightness = source.lightLevel;
            int objectBrightness = sourceBrightness - (int) objectDistance;

            if (objectBrightness > brightestRecordedLightLevel)
                brightestRecordedLightLevel = objectBrightness;

            if (brightestRecordedLightLevel == MaxLightLevel)
                break;
        }

        return brightestRecordedLightLevel;
    }

    public static void UpdateBlockLight(Location loc)
    {
        UpdateLightInArea(loc, loc);
    }

    public static void UpdateLightObject(LightObject lightObj)
    {
        Location loc = lightObj.GetLocation();
        List<LightSource> lightSources = GetLightSourcesForArea(loc + new Location(-MaxLightLevel, -MaxLightLevel),
            loc + new Location(MaxLightLevel, MaxLightLevel));

        UpdateLightObjectWithSpecificSources(lightObj, lightSources);
    }

    private static void UpdateLightObjectWithSpecificSources(LightObject lightObject, List<LightSource> possibleLightSources)
    {
        int lightLevel = GetLightLevel(lightObject.GetLocation(), possibleLightSources);

        lightObject.UpdateLightLevel(lightLevel);
    }
}

public struct LightColumn
{
    public int x;
    public Dimension dimension;

    public LightColumn(int x, Dimension dim)
    {
        this.x = x;
        dimension = dim;
    }
}
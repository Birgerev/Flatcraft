using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class LightManager : MonoBehaviour
{
    public static int maxLightLevel = 15;
    public static LightManager instance;

    public GameObject lightSourcePrefab;
    //public Dictionary<int, Location> sunlightSources = new Dictionary<int, Location>();
    
    void Start()
    {
        instance = this;
    }

    public void UpdateBlockLight(Location block)
    {
        UpdateLightInArea(new int2(block.x, block.y), new int2(block.x, block.y));
    }
    
    public void UpdateChunkLight(ChunkPosition chunk)
    {
        UpdateLightInArea(new int2(chunk.worldX, 0), new int2(chunk.worldX + Chunk.Width, Chunk.Height));
    }
    
    public void UpdateLightInArea(int2 min, int2 max)
    {
        List<LightObject> lightObjects = GetLightObjectsForArea(min, max);
        List<LightSource> lightSources = GetLightSourcesForArea(min - new int2(maxLightLevel, maxLightLevel), max + new int2(maxLightLevel, maxLightLevel));
        
        foreach(LightObject lightObject in lightObjects)
        {
            UpdateLight(lightObject, lightSources);
        }
    }

    public void UpdateLight(LightObject lightObject, List<LightSource> possibleLightSources)
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

    public List<LightObject> GetLightObjectsForArea(int2 boundingBoxMin, int2 boundingBoxMax)
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
    
    public List<LightSource> GetLightSourcesForArea(int2 boundingBoxMin, int2 boundingBoxMax)
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
    
    
    /*
    public static int GetLightLevel(Location loc)
    {
        //Messy layout due to multithreading
        List<Block> sources;
        Dictionary<int, Location> sunlightSourcesClone;
        lock (lightSources)
        {
            //clone actual list to avoid threading errors
            sources = new Dictionary<Block, int>(lightSources).Keys.ToList();
        }

        lock (sunlightSources)
        {
            //clone actual list to avoid threading errors
            sunlightSourcesClone = new Dictionary<int, Location>(sunlightSources);
        }

        var brightestSourceLoc = new Location(0, 0);
        var brightestValue = 0;

        foreach (var source in sources)
        {
            var sourceLoc = source.location;
            if (sourceLoc.dimension == loc.dimension)
            {
                var sourceBrightness = GetLightSourceLevel(source);
                var value = sourceBrightness - (int) math.distance(sourceLoc.GetPosition(), loc.GetPosition());

                if (value > brightestValue)
                {
                    brightestValue = value;
                    brightestSourceLoc = sourceLoc;
                }
            }
        }

        if (sunlightSourcesClone.ContainsKey(loc.x))
            //If current location y level is above sunlight source, return sunlight light level
            if (loc.y > sunlightSourcesClone[loc.x].y)
            {
                var isDay = WorldManager.world.time % WorldManager.dayLength < WorldManager.dayLength / 2;
                var sunlightLevel = isDay ? LightManager.maxLightLevel : 5;
                if (brightestValue < sunlightLevel)
                    brightestValue = sunlightLevel;
            }

        return brightestValue;
    }

    public static int GetLightSourceLevel(Block block)
    {
        if (block == null)
            return 0;

        var blockLevel = block.glowLevel;
        if (blockLevel == 0 && block.IsSunlightSource())
        {
            var isDay = WorldManager.world.time % WorldManager.dayLength < WorldManager.dayLength / 2;
            blockLevel = isDay ? LightManager.maxLightLevel : 5;
        }

        return blockLevel;
    }
    public static void UpdateLightAround(Location loc)
    {
        var chunk = new ChunkPosition(loc).GetChunk();
        if (chunk != null)
            chunk.lightSourcesToUpdate.Add(loc);
    }
    */
    
    /*public static void UpdateSunlightSourceAt(int x, Dimension dimension)
    {
        var topBlock = Chunk.GetTopmostBlock(x, dimension, false);
        if (topBlock == null)
            return;

        //remove all sunlight sources in the same column
        if (sunlightSources.ContainsKey(x))
        {
            var oldColumnSunlightSource = sunlightSources[x];
            var oldColumnSunlightSourceBlock = oldColumnSunlightSource.GetBlock();
            sunlightSources.Remove(x);
            if(oldColumnSunlightSourceBlock != null)
                lightSources.Remove(oldColumnSunlightSourceBlock);
            UpdateLightAround(oldColumnSunlightSource);
        }

        var isDay = WorldManager.world.time % WorldManager.dayLength < WorldManager.dayLength / 2;

        //Add the new position
        lightSources.Add(topBlock, isDay ? LightManager.maxLightLevel : 5);
        sunlightSources[topBlock.location.x] = topBlock.location;
        UpdateLightAround(topBlock.location);
    }

    public static void UpdateSunlightSourceAt(int x, Dimension dimension)
    {
        var topBlock = Chunk.GetTopmostBlock(x, dimension, false);
        if (topBlock == null)
            return;

        //remove all sunlight sources in the same column
        if (sunlightSources.ContainsKey(x))
        {
            var oldColumnSunlightSource = sunlightSources[x];
            var oldColumnSunlightSourceBlock = oldColumnSunlightSource.GetBlock();
            sunlightSources.Remove(x);
            if(oldColumnSunlightSourceBlock != null)
                lightSources.Remove(oldColumnSunlightSourceBlock);
            UpdateLightAround(oldColumnSunlightSource);
        }

        var isDay = WorldManager.world.time % WorldManager.dayLength < WorldManager.dayLength / 2;

        //Add the new position
        lightSources.Add(topBlock, isDay ? LightManager.maxLightLevel : 5);
        sunlightSources[topBlock.location.x] = topBlock.location;
        UpdateLightAround(topBlock.location);
    }*/
}

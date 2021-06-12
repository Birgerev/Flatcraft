using System.Collections.Generic;
using Unity.Burst;
using UnityEngine;

[BurstCompile]
public class LightManager : MonoBehaviour
{
    public static int maxLightLevel = 15;
    public static int nightLightLevel = 5;
    public static int netherLightLevel = 8;
    public static LightManager instance;

    public bool doLight = true;
    public GameObject lightSourcePrefab;
    public GameObject sunlightSourcePrefab;
    private bool doLightLastFrame = true;
    public Dictionary<BlockColumn, SunlightSource> sunlightSources = new Dictionary<BlockColumn, SunlightSource>();

    private void Start()
    {
        instance = this;
    }

    private void Update()
    {
        if (doLight != doLightLastFrame)
        {
            foreach (object chunkPos in WorldManager.instance.chunks.Keys)
                UpdateChunkLight((ChunkPosition) chunkPos);

            doLightLastFrame = doLight;
        }
    }

    public static bool DoesBlockInfluenceSunlight(Location loc)
    {
        BlockColumn column = new BlockColumn(loc.x, loc.dimension);

        if (instance.sunlightSources.ContainsKey(column))
            if (loc.y >= instance.sunlightSources[column].transform.position.y)
                return true;

        return false;
    }

    public static void UpdateSunlightInColumn(BlockColumn column, bool updateLight)
    {
        if (instance.sunlightSources.ContainsKey(column))
        {
            SunlightSource oldSunlightSource = instance.sunlightSources[column];
            Location oldSunlightLoc = oldSunlightSource.GetLocation();
            instance.sunlightSources.Remove(column);
            Destroy(oldSunlightSource.gameObject);

            //TODO why is 15 used, and not maxLightLevel const
            if (updateLight)
                UpdateLightInArea(oldSunlightLoc + new Location(-15, -15), oldSunlightLoc + new Location(15, 15));
        }

        //Dont create sunlight sources if player is in the nether
        if (column.dimension == Dimension.Nether)
            return;

        Block topmostBlock = Chunk.GetTopmostBlock(column.x, column.dimension, false);

        //Return in case no block was found in column, may be the case in ex void worlds
        if (topmostBlock == null)
            return;

        Location newSunlightLoc = topmostBlock.location;
        SunlightSource newSunlightSource =
            SunlightSource.Create(Location.LocationByPosition(topmostBlock.transform.position));

        instance.sunlightSources.Add(column, newSunlightSource);
        if (updateLight)
            UpdateLightInArea(newSunlightLoc + new Location(-15, -15), newSunlightLoc + new Location(15, 15));
    }

    public static void DestroySource(LightSource source)
    {
        Location loc = source.location;
        UpdateLightInArea(loc + new Location(-15, -15), loc + new Location(15, 15));
        Destroy(source.gameObject);
    }

    public static void UpdateBlockLight(Location loc)
    {
        UpdateLightInArea(loc, loc);
    }

    public static void UpdateChunkLight(ChunkPosition chunk)
    {
        UpdateLightInArea(new Location(chunk.worldX, 0, chunk.dimension),
            new Location(chunk.worldX + Chunk.Width, Chunk.Height, chunk.dimension));
    }

    public static void UpdateLightInArea(Location min, Location max)
    {
        List<LightObject> lightObjects = GetLightObjectsForArea(min, max);
        List<LightSource> lightSources = GetLightSourcesForArea(min + new Location(-maxLightLevel, -maxLightLevel),
            max + new Location(maxLightLevel, maxLightLevel));

        foreach (LightObject lightObject in lightObjects)
            UpdateLight(lightObject, lightSources);
    }

    public static void UpdateLightForSources(List<LightSource> sources)
    {
        HashSet<LightObject> lightObjects = new HashSet<LightObject>();

        foreach (LightSource source in sources)
        {
            Location sourceLoc = source.location;
            lightObjects.UnionWith(GetLightObjectsForArea(sourceLoc + new Location(-maxLightLevel, -maxLightLevel),
                sourceLoc + new Location(maxLightLevel, maxLightLevel)));
        }

        foreach (LightObject lightObject in lightObjects)
            UpdateLight(lightObject, sources);
    }

    public static void UpdateLightObject(LightObject lightObj)
    {
        Location loc = lightObj.GetLocation();
        List<LightSource> lightSources = GetLightSourcesForArea(loc + new Location(-maxLightLevel, -maxLightLevel),
            loc + new Location(maxLightLevel, maxLightLevel));

        UpdateLight(lightObj, lightSources);
    }

    public static void UpdateLight(LightObject lightObject, List<LightSource> possibleLightSources)
    {
        Vector3 objectPos = lightObject.GetPosition();
        int brightestRecordedLightLevel = 0;

        if (instance.doLight)
            foreach (LightSource source in possibleLightSources)
            {
                Vector3 sourcePos = source.position;
                float objectDistance = Vector3.Distance(sourcePos, objectPos);
                if (objectDistance > maxLightLevel)
                    continue;

                int sourceBrightness = source.lightLevel;
                int objectBrightness = sourceBrightness - (int) objectDistance;

                if (objectBrightness > brightestRecordedLightLevel)
                    brightestRecordedLightLevel = objectBrightness;

                if (brightestRecordedLightLevel == maxLightLevel)
                    break;
            }
        else
            brightestRecordedLightLevel = 15;

        lightObject.UpdateLightLevel(brightestRecordedLightLevel);
    }

    public static List<LightObject> GetLightObjectsForArea(Location boundingBoxMin, Location boundingBoxMax)
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

    public static List<LightSource> GetLightSourcesForArea(Location boundingBoxMin, Location boundingBoxMax)
    {
        Collider2D[] lightSourceColliders = Physics2D.OverlapAreaAll(boundingBoxMin.GetPosition(),
            boundingBoxMax.GetPosition(),
            LayerMask.GetMask("LightSource"));
        List<LightSource> lightSources = new List<LightSource>();

        foreach (Collider2D lightSourceCollider in lightSourceColliders)
            lightSources.Add(lightSourceCollider.GetComponent<LightSource>());

        return lightSources;
    }
}

public struct BlockColumn
{
    public int x;
    public Dimension dimension;

    public BlockColumn(int x, Dimension dim)
    {
        this.x = x;
        dimension = dim;
    }
}
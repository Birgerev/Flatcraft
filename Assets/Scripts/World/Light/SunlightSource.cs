using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunlightSource : MonoBehaviour
{
    public static Transform SunlightSourceParent;
    public LightSource lightSource;

    public void Start()
    {
        StartCoroutine(UpdateTimeOfDayLoop());
    }

    private TimeOfDay GetTimeOfDay()
    {
        return (WorldManager.instance.worldTime % WorldManager.dayLength > WorldManager.dayLength / 2) ? 
            TimeOfDay.Night : TimeOfDay.Day;
    }
    
    IEnumerator UpdateTimeOfDayLoop()
    {
        TimeOfDay lastUpdated = GetTimeOfDay();
        lightSource.UpdateLightLevel(
            GetTimeOfDay() == TimeOfDay.Night ? LightManager.nightLightLevel : LightManager.maxLightLevel, 
            false);

        while (true)
        {
            if(GetTimeOfDay() != lastUpdated)
            {
                lightSource.UpdateLightLevel(
                    GetTimeOfDay() == TimeOfDay.Night ? LightManager.nightLightLevel : LightManager.maxLightLevel,
                    true);
                lastUpdated = GetTimeOfDay();
            }

            yield return new WaitForSeconds(5);
        }
    }
    
    public Location GetLocation()
    {
        return Location.LocationByPosition(transform.position);
    }

    public static SunlightSource Create(Location loc)
    {
        if (SunlightSourceParent == null)
        {
            GameObject sunlightSourceParent = new GameObject("Sunlight Sources");
            SunlightSourceParent = sunlightSourceParent.transform;
        }
        GameObject obj = Instantiate(LightManager.instance.sunlightSourcePrefab, SunlightSourceParent);
        SunlightSource source = obj.GetComponent<SunlightSource>();
        
        obj.transform.position = loc.GetPosition();
        source.lightSource = LightSource.Create(obj.transform);
        
        return source;
    }
}

enum TimeOfDay
{
    Day,
    Night
}

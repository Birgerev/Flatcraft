using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunlightSource : MonoBehaviour
{
    public static Transform SunlightSourceParent;
    public LightSource lightSource;

    private void Start()
    {
        if (SunlightSourceParent == null)
        {
            GameObject sunlightSourceParent = new GameObject("Sunlight Sources");
            SunlightSourceParent = sunlightSourceParent.transform;
        }
        
        transform.SetParent(SunlightSourceParent);
        lightSource = GetComponent<LightSource>();
        
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
}

enum TimeOfDay
{
    Day,
    Night
}

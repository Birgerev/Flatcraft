using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunlightSource : MonoBehaviour
{
    public static Transform SunlightSourceParent;
    private LightSource lightSource;

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

    IEnumerator UpdateTimeOfDayLoop()
    {
        bool wasNightLastCheck = false;

        while (true)
        {
            var isNight = WorldManager.world.time % WorldManager.dayLength > WorldManager.dayLength / 2;

            if(isNight != wasNightLastCheck)
            {

                lightSource.UpdateLightLevel(isNight ? 5 : LightManager.maxLightLevel);
            }
            

            wasNightLastCheck = isNight;
            yield return new WaitForSeconds(5);
        }
    }
}

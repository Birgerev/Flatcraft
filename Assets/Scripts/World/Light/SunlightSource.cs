using System.Collections;
using UnityEngine;

public class SunlightSource : MonoBehaviour
{
    public static Transform SunlightSourceParent;
    public LightSource lightSource;

    public void StartTimeOfDayLoop()
    {
        StartCoroutine(UpdateTimeOfDayLoop());
    }

    private IEnumerator UpdateTimeOfDayLoop()
    {
        TimeOfDay lastUpdated = WorldManager.GetTimeOfDay();
        lightSource.UpdateLightLevel(
            WorldManager.GetTimeOfDay() == TimeOfDay.Night ? LightManager.NightLightLevel : LightManager.MaxLightLevel,
            false);

        while (true)
        {
            if (WorldManager.GetTimeOfDay() != lastUpdated)
            {
                lightSource.UpdateLightLevel(
                    WorldManager.GetTimeOfDay() == TimeOfDay.Night ? LightManager.NightLightLevel : LightManager.MaxLightLevel,
                    true);
                lastUpdated = WorldManager.GetTimeOfDay();
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

        GameObject obj = Instantiate(LightManager.Instance.sunlightSourcePrefab, SunlightSourceParent);
        SunlightSource source = obj.GetComponent<SunlightSource>();

        obj.transform.position = loc.GetPosition();
        source.lightSource = LightSource.Create(obj.transform);
        source.StartTimeOfDayLoop();

        return source;
    }
}
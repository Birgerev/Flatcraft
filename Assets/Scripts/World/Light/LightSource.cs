using UnityEngine;

public class LightSource : MonoBehaviour
{
    public LightValues lightValues;
    
    public Vector3 position;
    public Location location;

    public void UpdateLightWithinReach()
    {
        LightManager.UpdateLightInArea(
            location + new Location(-LightManager.MaxLightLevel, -LightManager.MaxLightLevel), 
            location + new Location(LightManager.MaxLightLevel, LightManager.MaxLightLevel));
    }

    public void UpdateLightValues(LightValues values, bool updateSurroundingLight)
    {
        lightValues = values;

        bool chunkLoaded = new ChunkPosition(location).IsChunkLoaded();

        if (updateSurroundingLight && chunkLoaded)
            UpdateLightWithinReach();
    }

    public static LightSource Create(Transform parent)
    {
        GameObject obj = Instantiate(LightManager.Instance.lightSourcePrefab, parent);
        LightSource source = obj.GetComponent<LightSource>();
        obj.transform.localPosition = Vector3.zero;

        source.position = obj.transform.position;
        source.location = Location.LocationByPosition(source.position);

        return source;
    }
}
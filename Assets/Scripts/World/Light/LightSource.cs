using UnityEngine;

public class LightSource : MonoBehaviour
{
    public int lightLevel;
    public Vector3 position;
    public Location location;

    public void UpdateLightWithinReach()
    {
        LightManager.UpdateLightInArea(
            location + new Location(-LightManager.MaxLightLevel, -LightManager.MaxLightLevel), 
            location + new Location(LightManager.MaxLightLevel, LightManager.MaxLightLevel));
    }

    public void UpdateLightLevel(int value, bool updateLight)
    {
        lightLevel = value;

        bool chunkLoaded = new ChunkPosition(location).IsChunkLoaded();

        if (updateLight && chunkLoaded)
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
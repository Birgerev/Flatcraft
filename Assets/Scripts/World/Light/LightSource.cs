using UnityEngine;

public class LightSource : MonoBehaviour
{
    public int lightLevel;
    public Vector3 position;
    public Location location;

    public void UpdateLight()
    {
        LightManager.UpdateLightInArea(location + new Location(-15, -15), location + new Location(15, 15));
    }

    public void UpdateLightLevel(int value, bool updateLight)
    {
        lightLevel = value;

        bool chunkLoaded = new ChunkPosition(location).IsChunkLoaded();

        if (updateLight && chunkLoaded)
            UpdateLight();
    }

    public static LightSource Create(Transform parent)
    {
        GameObject obj = Instantiate(LightManager.instance.lightSourcePrefab, parent);
        LightSource source = obj.GetComponent<LightSource>();
        obj.transform.localPosition = Vector3.zero;

        source.position = obj.transform.position;
        source.location = Location.LocationByPosition(source.position);

        return source;
    }
}
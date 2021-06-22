using UnityEngine;

public class LightObject : MonoBehaviour
{
    public int lightLevelDeduct;
    public bool staticObject;
    private Vector3 position;

    public Vector3 GetPosition()
    {
        if (staticObject)
        {
            if (position == Vector3.zero)
                position = transform.position;

            return position;
        }

        return position = transform.position;
    }

    public Location GetLocation()
    {
        return Location.LocationByPosition(GetPosition());
    }

    public void UpdateLightLevel(int lightLevel)
    {
        lightLevel -= lightLevelDeduct;
        lightLevel = Mathf.Clamp(lightLevel, 0, LightManager.maxLightLevel);

        Player player = Player.localEntity;
        if (player == null)
            return;

        if (GetLocation().dimension == Dimension.Nether)
            lightLevel = Mathf.Clamp(lightLevel, LightManager.netherLightLevel, int.MaxValue);

        float lightLevelFactor = lightLevel / (float) LightManager.maxLightLevel;
        Color color = new Color(lightLevelFactor, lightLevelFactor, lightLevelFactor, 1);

        GetComponent<SpriteRenderer>().color = color;
    }
}
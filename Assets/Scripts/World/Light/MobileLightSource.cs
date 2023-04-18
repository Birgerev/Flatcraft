public class MobileLightSource : LightSource
{
    private Location _lastFrameLocation;

    private void Update()
    {
        //If we've moved since last frame
        if (!GetLocation().Equals(_lastFrameLocation))
        {
            //Update light at new location
            UpdateLightWithinReach();
            //Update light at previous location
            UpdateLightPreviouslyWithinReach();
        }
    }
    
    public void UpdateLightPreviouslyWithinReach()
    {
        LightManager.UpdateLightInArea(
            _lastFrameLocation + new Location(-LightManager.MaxLightLevel, -LightManager.MaxLightLevel), 
            _lastFrameLocation + new Location(LightManager.MaxLightLevel, LightManager.MaxLightLevel));
    }

    public override Location GetLocation()
    {
        //Override caching behaviour present in regular light source
        return Location.LocationByPosition(transform.position);
    }

    private void LateUpdate()
    {
        //Store last frame location
        _lastFrameLocation = GetLocation();
    }
}
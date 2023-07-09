using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingLightSource : LightSource
{
    // Update is called once per frame
    void Update()
    {
        Location currentLoc = Location.LocationByPosition(transform.position);

        if (!_cachedLocation.Equals(currentLoc))
        {
            _cachedLocation = currentLoc;
            UpdateLightWithinReach();
        }
    }
}

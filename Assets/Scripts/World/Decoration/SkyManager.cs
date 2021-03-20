using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class SkyManager : MonoBehaviour
{
    public List<DimensionSky> skyForDimension = new List<DimensionSky>();

    void Update()
    {
        //Every second, update active sky object
        if ((Time.time % 1f) - Time.deltaTime <= 0)
        {
            Dimension dim = Dimension.Overworld;
            if (Player.localEntity != null)
                dim = Player.localEntity.Location.dimension;
            
            foreach (DimensionSky sky in skyForDimension)
            {
                sky.skyObject.SetActive(sky.dimension == dim);
            }
        }

        var cameraPosition = CameraController.instance.transform.position;
        transform.position = new Vector3(cameraPosition.x, transform.position.y);
    }
}

[Serializable]
public struct DimensionSky
{
    public Dimension dimension;
    public GameObject skyObject;
}
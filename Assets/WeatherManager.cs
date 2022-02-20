using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class WeatherManager : NetworkBehaviour
{
    public static WeatherManager instance;
    public GameObject rainPrefab;
    
    [SyncVar] public Weather weather;
    private GameObject rainEffect;

    private void Start()
    {
        instance = this;
        Instantiate(rainPrefab);//TODO
    }

    private void Update()
    {
        if (isServer)
        {
            //Increment weather time
            WorldManager.world.weatherTime -= Time.deltaTime;

            if (WorldManager.world.weatherTime <= 0)
            {
                ChangeWeather();
            }
            
            //Update syncVars
            if (Time.time % 0.5f - Time.deltaTime <= 0)
            {
                weather = WorldManager.world.weather;
            }
        }

        if (weather == Weather.Raining)
        {
            //rainPrefab.GetComponent<ParticleSystem>().emission.
        }
    }

    public void ChangeWeather()
    {
        //TODO support for multiple weathers
        WorldManager.world.weather = (WorldManager.world.weather == Weather.Clear) ? Weather.Raining : Weather.Clear;
        WorldManager.world.weatherTime = NewWeatherTime(WorldManager.world.weather);
    }
    
    public static float NewWeatherTime(Weather weather)
    {
        System.Random r = new System.Random();
        switch (weather)
        {
            case Weather.Clear:
                return WorldManager.DayLength * r.Next(1, 7);
            case Weather.Raining:
                return WorldManager.DayLength * (0.5f + (float)(r.NextDouble()*0.5f));
        }

        return 0;
    }
}

using UnityEngine;

public class OverworldSky : MonoBehaviour
{
    public GameObject playerLockedY;
    public GameObject skySpinner;
    public GameObject sun;
    public GameObject moon;



    // Update is called once per frame
    private void Update()
    {
        //Calculate time of day value between 0 and 1
        float timeOfDay01 = WorldManager.instance.worldTime % WorldManager.DayLength / WorldManager.DayLength;
        
        //Move Lock-Y to camera y position
        Vector3 cameraPosition = CameraController.instance.transform.position;
        playerLockedY.transform.position = new Vector3(transform.position.x, cameraPosition.y);
        
        //Sky rotation
        skySpinner.transform.rotation = Quaternion.Euler(0, 0, -timeOfDay01 * 360);
        sun.transform.rotation = Quaternion.identity;
        moon.transform.rotation = Quaternion.identity;
        
        //Sky Animator update values
        GetComponent<Animator>().SetFloat("time", timeOfDay01 * 100);
        GetComponent<Animator>().SetBool("raining", (WeatherManager.instance.weather == Weather.Raining));
    }
}
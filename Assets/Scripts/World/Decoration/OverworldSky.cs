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
        float timeOfDay01 = WorldManager.instance.worldTime % WorldManager.DayLength / WorldManager.DayLength;
            
        Vector3 cameraPosition = CameraController.instance.transform.position;
        playerLockedY.transform.position = new Vector3(transform.position.x, cameraPosition.y);
        
        skySpinner.transform.rotation = Quaternion.Euler(0, 0, -timeOfDay01 * 360);
        sun.transform.rotation = Quaternion.identity;
        moon.transform.rotation = Quaternion.identity;
        
        GetComponent<Animator>().SetFloat("time", timeOfDay01 * 100);
    }
}
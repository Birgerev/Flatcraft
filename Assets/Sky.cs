using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sky : MonoBehaviour
{
    public static float sunlightIntensity;

    public GameObject playerLockedY;
    public GameObject skySpinner;
    public GameObject Sun;
    public GameObject Moon;

    private float timeOfDay;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        timeOfDay = (WorldManager.world.time % WorldManager.dayLength) / WorldManager.dayLength;

        Vector3 cameraPosition = CameraController.instance.transform.position;

        playerLockedY.transform.position = new Vector3(transform.position.x, cameraPosition.y);

        skySpinner.transform.rotation = Quaternion.Euler(0, 0, -timeOfDay * 360);

        transform.position = new Vector3(cameraPosition.x, transform.position.y);

        Sun.transform.rotation = Quaternion.identity;
        Moon.transform.rotation = Quaternion.identity;

        lightLevels();

        GetComponent<Animator>().SetFloat("time", timeOfDay);
    }

    void lightLevels()
    {
        float intensity = (timeOfDay > 0.55f && timeOfDay < 0.95f) ? 0.1f : 1;

        sunlightIntensity = intensity;
    }
}

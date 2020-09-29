using UnityEngine;

public class Sky : MonoBehaviour
{
    public static float sunlightIntensity;
    public GameObject Moon;

    public GameObject playerLockedY;
    public GameObject skySpinner;
    public GameObject Sun;

    private float timeOfDay;

    // Start is called before the first frame update
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
        timeOfDay = WorldManager.world.time % WorldManager.dayLength / WorldManager.dayLength;

        var cameraPosition = CameraController.instance.transform.position;

        playerLockedY.transform.position = new Vector3(transform.position.x, cameraPosition.y);

        skySpinner.transform.rotation = Quaternion.Euler(0, 0, -timeOfDay * 360);

        transform.position = new Vector3(cameraPosition.x, transform.position.y);

        Sun.transform.rotation = Quaternion.identity;
        Moon.transform.rotation = Quaternion.identity;

        lightLevels();

        GetComponent<Animator>().SetFloat("time", timeOfDay);
    }

    private void lightLevels()
    {
        var intensity = timeOfDay > 0.55f && timeOfDay < 0.95f ? 0.1f : 1;

        sunlightIntensity = intensity;
    }
}
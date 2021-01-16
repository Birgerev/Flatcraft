using UnityEngine;

public class OverworldSky : MonoBehaviour
{
    public GameObject Moon;

    public GameObject playerLockedY;
    public GameObject skySpinner;
    public GameObject Sun;

    private float timeOfDay;

    // Update is called once per frame
    private void Update()
    {
        var cameraPosition = CameraController.instance.transform.position;

        playerLockedY.transform.position = new Vector3(transform.position.x, cameraPosition.y);
        skySpinner.transform.rotation = Quaternion.Euler(0, 0, -timeOfDay * 360);

        Sun.transform.rotation = Quaternion.identity;
        Moon.transform.rotation = Quaternion.identity;

        timeOfDay = WorldManager.world.time % WorldManager.dayLength / WorldManager.dayLength;
        GetComponent<Animator>().SetFloat("time", timeOfDay * 100);
    }
}
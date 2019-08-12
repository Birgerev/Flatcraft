using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController instance;

    //Camera target variables
    public float dampTime = 0.15f;
    public float zoomDampTime = 2f;
    private Vector3 velocity = Vector3.zero;
    private float vel = 0f;
    public static float targetFov = 20f;
    public Transform target;

    public const float normalFov = 20f;
    public const float zoomedFov = 16f;

    private float startZ = -0.1f;

    //Camera shake variables
    public float maxRoll = 10;
    public float maxOffset = 0.5f;
    public float shake = 0;
    public float shakedropoffPerFrame = 0.8f;
    public float shakeforce = 1.5f;

    public float roll;
    public float offsetX;
    public float offsetY;

    int seed = 0;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;

        if (startZ == -0.1f)
        {
            startZ = transform.position.z;
        }
    }

    void ShakeCalc()
    {
        //Seeds needs to use fractal numbers
        roll = maxRoll * shake * Mathf.PerlinNoise(seed + 0.4f, seed + 0.4f);
        offsetX = maxOffset * shake * Mathf.PerlinNoise(seed + 1.4f, seed + 1.4f);
        offsetY = maxOffset * shake * Mathf.PerlinNoise(seed + 2.4f, seed + 2.4f);

        seed += 1;

        shake *= shakedropoffPerFrame;
        if (shake < 0.001f)
            shake = 0;
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        //Smoothly target player
        transform.position = Vector3.SmoothDamp(transform.position, target.position, ref velocity, dampTime);
        
        float currentFov = GetComponent<Camera>().orthographicSize;


        GetComponent<Camera>().orthographicSize = Mathf.SmoothDamp(currentFov, targetFov, ref vel, zoomDampTime);


        ShakeCalc();

        Vector3 pos = transform.position;
        float angle = 0;

        pos += new Vector3(offsetX, offsetY);
        angle = roll;

        pos.z = startZ;

        transform.position = pos;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}

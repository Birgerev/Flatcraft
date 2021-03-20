using UnityEngine;

public class CameraController : MonoBehaviour
{
    public const float normalFov = 20f;
    public const float zoomedFov = 16f;
    public static CameraController instance;
    public static float targetFov = 20f;

    //Camera target variables
    public float dampTime = 0.15f;
    public float maxOffset = 0.5f;

    //Camera shake variables
    public float maxRoll = 10;
    public float offsetX;
    public float offsetY;

    public float roll;

    private int seed;
    public float shake;
    public float shakedropoffPerFrame = 0.8f;
    public float shakeforce = 1.5f;

    private float startZ = -0.1f;
    public Transform target;
    private float vel;
    private Vector3 velocity = Vector3.zero;
    public float zoomDampTime = 2f;

    // Start is called before the first frame update
    private void Start()
    {
        instance = this;

        if (startZ == -0.1f) startZ = transform.position.z;
    }

    private void ShakeCalc()
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
    private void FixedUpdate()
    {
        if (Player.localEntity == null)
            return;
        if (target == null)
            target = Player.localEntity.transform;

        //Smoothly target player
        transform.position = Vector3.SmoothDamp(transform.position, target.position, ref velocity, dampTime);

        var currentFov = GetComponent<Camera>().orthographicSize;


        GetComponent<Camera>().orthographicSize = Mathf.SmoothDamp(currentFov, targetFov, ref vel, zoomDampTime);


        ShakeCalc();

        var pos = transform.position;
        float angle = 0;

        pos += new Vector3(offsetX, offsetY);
        angle = roll;

        pos.z = startZ;

        transform.position = pos;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}
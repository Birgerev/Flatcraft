using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController instance;
    
    //Camera target variables
    [Header("Targeting Values")]
    public float dampTime = 0.15f;
    public Transform target;
    private Vector3 currentTargetSmoothVelocity;

    //Camera transform variables
    [Header("Transform Values")]
    public float offsetX;
    public float offsetY;
    public float roll;

    //Shake
    [Header("Shake Values")]
    public float shake;
    public float shakeDropoffPerFrame = 0.8f;
    public float shakeSpeed = 0.8f;
    public float maxOffset = 0.5f;
    public float maxRoll = 10;

    [Header("Zoom Values")]
    public float zoomDampTime = 2f;
    private float currentSmoothZoomVelocity;
    public int roofCheckMaxDistance = 5;
    public float normalFov = 11f;
    public float zoomedFov = 8f;
    public static float targetFov;

    
    // Start is called before the first frame update
    private void Start()
    {
        instance = this;
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        if (target == null)
            return;
        
        //Smoothly target player
        CalculateShake();
        SetTranslation();
        CalculateTargetZoom();
        SetSmoothTargetZoom();
    }
    
    private void CalculateShake()
    {
        //Seeds needs to use fractal numbers
        roll = maxRoll * shake * (Mathf.PerlinNoise(0f, Time.time * shakeSpeed) - 0.5f);
        offsetX = maxOffset * shake * (Mathf.PerlinNoise(100f, Time.time * shakeSpeed) - 0.5f);
        offsetY = maxOffset * shake * (Mathf.PerlinNoise(200f, Time.time * shakeSpeed) - 0.5f);

        shake *= shakeDropoffPerFrame;
        if (shake < 0.001f)
            shake = 0;
    }

    private void CalculateTargetZoom()
    {
        if ((Time.time % 0.5f) - Time.deltaTime <= 0)
        {
            targetFov = normalFov;
            
            for (int y = 0; y < roofCheckMaxDistance; y++)
            {
                Block block = Location.LocationByPosition(target.transform.position + new Vector3(0, y)).GetBlock();

                if (block != null && block.solid)
                {
                    targetFov = zoomedFov;

                    break;
                }
            }
        }
    }

    private void SetSmoothTargetZoom()
    {
        float currentFov = GetComponent<Camera>().orthographicSize;
        
        GetComponent<Camera>().orthographicSize = Mathf.SmoothDamp(
            currentFov, 
            targetFov, 
            ref currentSmoothZoomVelocity, 
            zoomDampTime);
    }

    private void SetTranslation()
    {
        transform.position = Vector3.SmoothDamp(
            transform.position, 
            target.position + new Vector3(offsetX, offsetY, -10), 
            ref currentTargetSmoothVelocity, 
            dampTime);
        transform.rotation = Quaternion.Euler(0, 0, roll);
    }
}
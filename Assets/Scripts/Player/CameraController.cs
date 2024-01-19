using UnityEngine;
using UnityEngine.Serialization;

public class CameraController : MonoBehaviour
{
    public static CameraController instance;

    //Camera target variables
    [Header("Targeting Values")] 
    public float dampTime = 0.15f;
    public Transform target;

    //Camera transform variables
    [Header("Transform Values")] 
    public Vector2 defaultOffset;

    //Shake
    [Header("Shake Values")] 
    public float shakeDropoffFactorPerFrame = 0.8f;
    public float shakeSpeed = 0.8f;
    public float maxOffset = 0.5f;
    public float maxRoll = 10;

    [Header("Zoom Values")] 
    public float zoomDampTime = 2f;
    public float normalFov;
    public float sprintZoomAmount;
    public float sneakZoomAmount;
    public float roofFov;
    public int roofCheckMaxDistance = 5;

    [Header("Mouse Panning")] 
    public Vector2 mousePanningMagnitude;
    
    [HideInInspector] public float currentShake;
    
    private float _currentRoll;
    private float _currentSmoothZoomVelocity;
    private Vector3 _currentTargetSmoothVelocity;
    private Vector2 _currentShakeOffset;
    private Vector2 _currentMousePanOffset;
    private float _targetFov;

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
        CalculateMousePanning();
        CalculateShakeThisFrame();
        SetTranslation();
        
        //Smoothly zoom
        CalculateWhichZoomToTarget();
        SmoothToTargetZoom();
    }
    
    
    private void CalculateShakeThisFrame()
    {
        //Seeds needs to use fractal numbers
        _currentRoll = maxRoll * currentShake * (Mathf.PerlinNoise(0f, Time.time * shakeSpeed) - 0.5f);
        _currentShakeOffset.x = maxOffset * currentShake * (Mathf.PerlinNoise(100f, Time.time * shakeSpeed) - 0.5f);
        _currentShakeOffset.y = maxOffset * currentShake * (Mathf.PerlinNoise(200f, Time.time * shakeSpeed) - 0.5f);

        currentShake *= shakeDropoffFactorPerFrame;
        if (currentShake < 0.001f)
            currentShake = 0;
    }
    
    private void CalculateMousePanning()
    {
        //Mouse position between 0 and 1
        Vector2 mouseScreenPosition01 = Input.mousePosition / new Vector2(Screen.width, Screen.height);
        //Mouse position between -1 and 1, with 0 being centre
        Vector2 mouseScreenPositionCentered = (mouseScreenPosition01 * 2) + new Vector2(-1, -1);
        
        _currentMousePanOffset = mousePanningMagnitude * mouseScreenPositionCentered;
    }

    private void CalculateWhichZoomToTarget()
    {
        _targetFov = normalFov;

        //Look for blocks over player head
        for (int y = 0; y < roofCheckMaxDistance; y++)
        {
            Block block = Location.LocationByPosition(target.transform.position + new Vector3(0, y)).GetBlock();

            if (block == null) continue;
            if (!block.IsSolid) continue;
            
            _targetFov = roofFov;
            break;
        }

        if (Player.LocalEntity.GetComponent<PlayerMovement>().sprinting) _targetFov += sprintZoomAmount;
        if(Player.LocalEntity.GetComponent<PlayerMovement>().sneaking) _targetFov += sneakZoomAmount;
    }

    private void SmoothToTargetZoom()
    {
        float currentFov = GetComponent<Camera>().orthographicSize;

        GetComponent<Camera>().orthographicSize = Mathf.SmoothDamp(currentFov,
            _targetFov,
            ref _currentSmoothZoomVelocity,
            zoomDampTime);
    }

    private void SetTranslation()
    {
        Vector3 targetPosition = target.position + new Vector3(0 , 0, -10);
        targetPosition += (Vector3)defaultOffset;
        targetPosition += (Vector3)_currentMousePanOffset;
        
        //Smoothly move towards target position
        transform.position = 
            Vector3.SmoothDamp(transform.position, targetPosition, ref _currentTargetSmoothVelocity,
            dampTime);
        
        //Add non smoothed offsets
        transform.position += (Vector3) _currentShakeOffset;
        
        //Assign current roll
        transform.rotation = Quaternion.Euler(0, 0, _currentRoll);
    }
    
    public static void ShakeClientCamera(float shakeValue)
    {
        instance.currentShake = shakeValue;
    }
}
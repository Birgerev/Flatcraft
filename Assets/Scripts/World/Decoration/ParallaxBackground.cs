using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    public Vector2 parallaxFactor;
    [HideInInspector]
    public Vector2 startPosition;
    
    private Transform cameraTransform;
    private float backgroundSpriteWidth;

    // Start is called before the first frame update
    private void Start()
    {
        startPosition = transform.position;
        backgroundSpriteWidth = GetComponentInChildren<SpriteRenderer>().bounds.size.x;
    }

    // Update is called once per frame
    private void Update()
    {
        //Assign camera transform
        if (cameraTransform == null)
            cameraTransform = CameraController.instance.transform;

        //Move transform with respect to the parallaxFactor
        Vector2 cameraDeltaPos = (Vector2)cameraTransform.position - startPosition;
        transform.position = startPosition + (cameraDeltaPos * parallaxFactor);

        HorizontalWrapAround();
    }

    private void HorizontalWrapAround()
    {
        //Move the object offset if the camera is further away than the sprite bound length
        float cameraXAhead = cameraTransform.position.x - transform.position.x;
        if (cameraXAhead > backgroundSpriteWidth)
            startPosition += new Vector2(backgroundSpriteWidth, 0);
        else if (cameraXAhead < -backgroundSpriteWidth)
            startPosition += new Vector2(-backgroundSpriteWidth, 0);
    }
}
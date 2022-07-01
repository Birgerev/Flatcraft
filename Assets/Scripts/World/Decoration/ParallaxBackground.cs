using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    public Vector2 parallaxFactor;
    private Transform cameraTransform;

    private float backgroundSpriteLength;
    private Vector2 startPosition;

    // Start is called before the first frame update
    private void Start()
    {
        startPosition = transform.position;
        backgroundSpriteLength = GetComponentInChildren<SpriteRenderer>().bounds.size.x;
    }

    // Update is called once per frame
    private void Update()
    {
        //Assign camera transform
        if (cameraTransform == null)
            cameraTransform = CameraController.instance.transform;

        //Move transform with respect to the parallaxFactor
        Vector2 distanceFromStartPosition = cameraTransform.position * parallaxFactor;
        transform.position = startPosition + distanceFromStartPosition;

        //Move the object offset if the camera is further away than the sprite bound length
        float cameraXAhead = cameraTransform.position.x - transform.position.x;
        if (cameraXAhead > backgroundSpriteLength)
            startPosition += new Vector2(backgroundSpriteLength, 0);
        else if (cameraXAhead < -backgroundSpriteLength)
            startPosition += new Vector2(-backgroundSpriteLength, 0);
    }
}
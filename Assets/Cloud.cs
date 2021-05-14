using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cloud : MonoBehaviour
{
    public Vector2 parallaxFactor;
    public float cameraYOffset;
    public float windTravelSpeed;
    
    private float cloudSpriteLength;
    private Transform cameraTransform;
    private Vector2 startPosition;

    // Start is called before the first frame update
    void Start()
    {
        startPosition = transform.position;
        cloudSpriteLength = GetComponentInChildren<SpriteRenderer>().bounds.size.x;
    }

    // Update is called once per frame
    void Update()
    {
        //Assign camera transform
        if(cameraTransform == null)
            cameraTransform = CameraController.instance.transform;
        
        //Move by wind
        startPosition += new Vector2(windTravelSpeed * Time.deltaTime, 0);

        //Move transform with respect to the parallaxFactor
        Vector2 distanceFromStartPosition = cameraTransform.position * parallaxFactor;
        transform.position = startPosition + distanceFromStartPosition - new Vector2(0, cameraYOffset);

        //Move the object offset if the camera is further away than the sprite bound length
        float cameraXAhead = (transform.position.x * (1 - parallaxFactor.x));
        if (cameraXAhead > startPosition.x + cloudSpriteLength) startPosition += new Vector2(cloudSpriteLength, 0);
        else if (cameraXAhead < startPosition.x - cloudSpriteLength) startPosition += new Vector2(-cloudSpriteLength, 0);
    }
}

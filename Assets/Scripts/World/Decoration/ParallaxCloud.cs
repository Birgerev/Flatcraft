using UnityEngine;

public class ParallaxCloud : MonoBehaviour
{
    public float windTravelSpeed;
    
    // Update is called once per frame
    private void Update()
    {
        //Move by wind
        GetComponent<ParallaxBackground>().startPosition += new Vector2(windTravelSpeed * Time.deltaTime, 0);
    }
}
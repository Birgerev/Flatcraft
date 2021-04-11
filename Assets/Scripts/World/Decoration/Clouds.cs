using System;
using System.Collections.Generic;
using UnityEngine;

public class Clouds : MonoBehaviour
{
    public GameObject cloudPrefab;
    public List<GameObject> clouds = new List<GameObject>();
    public Color color;
    public float cloudWidth;
    public float cloudSpeed;

    public void Start()
    {
        GameObject mainCloud = CreateCloud();
        mainCloud.transform.localPosition = new Vector3(0, 0);
        clouds.Add(mainCloud);
        
        GameObject leftCloud = CreateCloud();
        leftCloud.transform.localPosition = new Vector3(-cloudWidth, 0);
        clouds.Add(leftCloud);
    }


    // Update is called once per frame
    private void Update()
    {
        foreach (GameObject cloud in clouds)
        {
            cloud.GetComponent<SpriteRenderer>().color = color;
            cloud.transform.position += new Vector3(cloudSpeed * Time.deltaTime, 0);
        }

        if (clouds[0].transform.localPosition.x >= cloudWidth)
        {
            GameObject oldestCloud = clouds[0];
            clouds.Remove(oldestCloud);
            Destroy(oldestCloud);
            
            GameObject newCloud = CreateCloud();
            newCloud.transform.localPosition = new Vector3(-cloudWidth, 0);
            clouds.Add(newCloud);
        }
    }

    public GameObject CreateCloud()
    {
        return Instantiate(cloudPrefab, transform);
    }
}
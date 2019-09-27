using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clouds : MonoBehaviour
{
    public GameObject cloud;

    private float cloudDistance = 30;
    private float cloudSpeed = 0.5f;
    private float cloudChance = 50;
    private float cloudHeight = 128;
    private float cloudSpawnInterval = 7.9f;


    private float nextSpawnTime = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Player.localInstance == null)
            return;
        Vector2 playerPos = Player.localInstance.transform.position;

        if (Time.time > nextSpawnTime)
        {
            if (Random.Range(0f, 100f) < cloudChance)
            {
                CreateCloud();
            }
            nextSpawnTime = Time.time + cloudSpawnInterval;
        }


        for(int i = 0; i < transform.childCount; i++)
        {
            //Clear cloud
            if (transform.GetChild(i).position.x > playerPos.x + cloudDistance * 2)
                Destroy(transform.GetChild(i).gameObject);

            //Move
            transform.GetChild(i).position += new Vector3(cloudSpeed * Time.fixedDeltaTime, 0); 
        }
    }

    public void CreateCloud()
    {
        GameObject obj = Instantiate(cloud);

        obj.transform.SetParent(transform);
        obj.transform.position = new Vector2(Player.localInstance.transform.position.x - cloudDistance, cloudHeight);
    }
}

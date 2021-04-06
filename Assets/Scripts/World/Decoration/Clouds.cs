using UnityEngine;

public class Clouds : MonoBehaviour
{
    public GameObject cloud;
    private readonly float cloudChance = 50;

    private readonly float cloudDistance = 30;
    private readonly float cloudHeight = 128;
    private readonly float cloudSpawnInterval = 7.9f;
    private readonly float cloudSpeed = 0.5f;


    private float nextSpawnTime;

    // Start is called before the first frame update
    private void Start()
    {
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        if (Player.localEntity == null)
            return;
        Vector2 playerPos = Player.localEntity.transform.position;

        if (Time.time > nextSpawnTime)
        {
            if (Random.Range(0f, 100f) < cloudChance) CreateCloud();
            nextSpawnTime = Time.time + cloudSpawnInterval;
        }


        for (var i = 0; i < transform.childCount; i++)
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
        var obj = Instantiate(cloud);

        obj.transform.SetParent(transform);
        obj.transform.position = new Vector2(Player.localEntity.transform.position.x - cloudDistance, cloudHeight);
    }
}
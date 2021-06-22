using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class BreakIndicator : NetworkBehaviour
{
    public static Dictionary<Location, BreakIndicator> breakIndicators = new Dictionary<Location, BreakIndicator>();
    [SyncVar] public Location loc;
    [SyncVar] public float blockHealth;
    [SyncVar] public float maxBlockHealth;
    private float lastFrameBlockHealth = int.MaxValue;

    public void Start()
    {
        breakIndicators.Add(loc, this);
    }

    // Update is called once per frame
    private void Update()
    {
        if (isServer && Time.time % 0.2f - Time.deltaTime <= 0)
        {
            Block block = loc.GetBlock();
            if (block != null)
            {
                blockHealth = block.blockHealth;
                maxBlockHealth = block.breakTime;
            }

            CheckDespawn();
        }

        transform.position = loc.GetPosition();
        UpdateState();

        lastFrameBlockHealth = blockHealth;
    }

    public void OnDestroy()
    {
        breakIndicators.Remove(loc);
    }

    public void UpdateState()
    {
        if (maxBlockHealth == 0)
            return;

        Sprite[] sprites = Resources.LoadAll<Sprite>("Sprites/Block_Break");
        int spriteIndex = (int) (blockHealth / maxBlockHealth * (sprites.Length - 1));
        Sprite sprite = sprites[spriteIndex];

        GetComponent<SpriteRenderer>().sprite = sprite;
    }

    [Server]
    public void Unspawn()
    {
        NetworkServer.Destroy(gameObject);
    }

    [Server]
    public void CheckDespawn()
    {
        Block block = loc.GetBlock();

        if (block == null || blockHealth == maxBlockHealth)
            Unspawn();
    }

    [Server]
    public static void Spawn(Location loc)
    {
        GameObject obj = Instantiate(Resources.Load<GameObject>("Prefabs/BreakIndicator"));
        BreakIndicator indicator = obj.GetComponent<BreakIndicator>();

        indicator.loc = loc;
        NetworkServer.Spawn(obj);
    }
}
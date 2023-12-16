using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Serialization;

public class BreakIndicator : NetworkBehaviour
{
    public static Dictionary<Location, BreakIndicator> breakIndicators = new Dictionary<Location, BreakIndicator>();
    [SyncVar] public Location loc;
    [SyncVar] public float blockDamage;
    [SyncVar] public float maxBlockDamage;

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
                blockDamage = block.blockDamage;
                maxBlockDamage = block.BreakTime;
            }

            CheckDespawn();
        }

        transform.position = loc.GetPosition();
        UpdateState();
    }

    public void OnDestroy()
    {
        breakIndicators.Remove(loc);
    }

    public void UpdateState()
    {
        if (maxBlockDamage == 0)
            return;

        Sprite[] sprites = Resources.LoadAll<Sprite>("Sprites/Block_Break");
        float breakProgress = blockDamage / maxBlockDamage;
        int spriteIndex = (int)( breakProgress * sprites.Length);
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

        if (block == null || blockDamage == 0)
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
using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class BreakIndicator : NetworkBehaviour
{
    public static Dictionary<Location, BreakIndicator> breakIndicators = new Dictionary<Location, BreakIndicator>();
    [SyncVar] public Location loc;
    private float lastBlockHealth;

    public void Start()
    {
        breakIndicators.Add(loc, this);
    }

    public void OnDestroy()
    {
        breakIndicators.Remove(loc);
    }

    // Update is called once per frame
    private void Update()
    {
        transform.position = loc.GetPosition();

        if ((Time.time % 0.2f) - Time.deltaTime <= 0)
        {
            Block block = loc.GetBlock();

            if (block == null)
            {
                if(isServer)
                    Unspawn();

                return;
            }
            
            if (block.blockHealth == lastBlockHealth)
            {
                if(isServer)
                    Unspawn();
            }
            else
            {
                UpdateState();
            }
        }
    }
    public void UpdateState()
    {
        Block block = loc.GetBlock();
        Sprite[] sprites = Resources.LoadAll<Sprite>("Sprites/Block_Break");
        int spriteIndex = (int) (block.blockHealth / block.breakTime / (1f / sprites.Length));

        Sprite sprite = null;
        if (spriteIndex < sprites.Length && spriteIndex >= 0)
            sprite = sprites[spriteIndex];

        GetComponent<SpriteRenderer>().sprite = sprite;
    }

    [Server]
    public void Unspawn()
    {
        NetworkServer.Destroy(gameObject);
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
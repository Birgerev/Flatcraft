using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Burnable : NetworkBehaviour
{
    public GameObject burningRender;
    
    private Entity _entity;

    private void Update()
    {
        DoFireRender();
        
        if (!isServer) return;
        
        if (_entity.fireTime > 0)
        {
            _entity.fireTime -= Time.deltaTime;
            WaterExtinguishFire();
        }
        
        CheckLavaDamage();
        CheckFireBlock();
        CheckFireDamage();
    }

    [Server]
    private void CheckFireBlock()
    {
        bool isInLava = false;
        foreach (Block block in _entity.GetBlocksForEntity())
            if (block is Fire)
            {
                _entity.fireTime = 7;
                break;
            }
    }

    [Client]
    private void DoFireRender()
    {
        if (!burningRender) return;
        
        burningRender.SetActive(_entity.fireTime > 0);
    }
    
    [Server]
    private void CheckFireDamage()
    {
        if (_entity.fireTime <= 0) return;
        if (Time.time % 1f > Time.deltaTime) return;
            
        _entity.Damage(1);
    }


    [Server]
    private void CheckLavaDamage()
    {
        if (_entity.isInLiquid)
        {
            bool isInLava = false;
            foreach (Liquid liquid in _entity.GetLiquidBlocksForEntity())
                if (liquid is Lava)
                {
                    isInLava = true;
                    break;
                }

            if (isInLava)
            {
                _entity.fireTime = 14;

                if (Time.time % 0.5f - Time.deltaTime <= 0)
                    _entity.Damage(4);
            }
        }
    }

    [Server]
    private void WaterExtinguishFire()
    {
        if (!_entity.isInLiquid) return;

        foreach (Liquid liquid in _entity.GetLiquidBlocksForEntity())
        {
            if (liquid is not Water) continue;
            
            _entity.fireTime = 0;
            return;
        }
    }
    
    private void Awake()
    {
        _entity = GetComponent<Entity>();
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    private const float SneakSpeed = 1.3f;
    private const float SprintSpeed = 5.6f;
    
    [SyncVar] public bool sprinting;
    [SyncVar] public bool sneaking;
    
    private float _lastDoubleTapSprintTap;
    private bool _ladderSneakingLastFrame;
    
    private Player _player;
    private EntityMovement _movement;

    //TODO clean
    private void Update()
    {
        if (!isOwned) return;
        
        PerformInput();
        CrouchOnLadderCheck();
    }

    [Client]
    private void PerformInput()
    {
        if (!PlayerInteraction.CanInteractWithWorld()) return;
        
        //Toggle Debug disabled lighting
        if (Input.GetKeyDown(KeyCode.F4) && Debug.isDebugBuild)
        {
            LightManager.DoLight = !LightManager.DoLight;
            LightManager.UpdateAllLight();
        }
        
        //Open chat
        if (Input.GetKeyDown(KeyCode.T))
            ChatMenu.instance.open = true;
        
        //Jumping
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.Space))
            _movement.Jump();

        WalkInput();
        SprintInput();
        SneakInput();
    }

    private void WalkInput()
    {
        int direction = 0;
        if (Input.GetKey(KeyCode.A)) direction = -1;
        if (Input.GetKey(KeyCode.D)) direction = 1;
        if (direction == 0) return;

        //Stop from falling off edge when sneaking
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (sneaking && WillFallInDirection(direction)) { rb.velocity = new Vector2(0, rb.velocity.y); return;}
        
        _movement.Walk(direction);
    }

    private void SprintInput()
    {
        //Stop Sprinting
        if (sprinting &&
            (Mathf.Abs(_player.GetVelocity().x) < 0.1f || 
             sneaking || 
             _player.hunger <= 6 || 
             (!Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D))))
        {
            sprinting = false;
            _movement.speed = _movement.walkSpeed;
        }
        
        //CTRL start sprint
        if (!sprinting && Input.GetKey(KeyCode.LeftControl) && _player.hunger > 6 && !sneaking)
        {
            sprinting = true;
            _movement.speed = SprintSpeed;
        }
        
        //Double press walk start sprint
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D))
        {
            if (Time.time - _lastDoubleTapSprintTap < 0.3f &&  !sneaking)
            {
                sprinting = true;
                _movement.speed = SprintSpeed;
            }
            
            _lastDoubleTapSprintTap = Time.time;
        }
    }

    private void SneakInput()
    {
        //Sneaking
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.S))
        {
            sneaking = true;
            _movement.speed = SneakSpeed;
        }
        
        //Stop sneaking
        if (sneaking && (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.S)))
        {
            sneaking = false;
            _movement.speed = _movement.walkSpeed;
        }
    }

    private bool WillFallInDirection(int direction)
    {
        if (!_player.isOnGround) return false;
        
        Block nextGroundBlock = Location.LocationByPosition(transform.position + new Vector3(direction * .1f, -.6f)).GetBlock();
        
        if(!nextGroundBlock) return true;
        if(!nextGroundBlock.IsSolid) return true;

        return false;
    }

    private void CrouchOnLadderCheck()
    {
        bool isLadderSneaking = _player.isOnClimbable && sneaking;
        
        //Started ladder sneaking
        if (isLadderSneaking && !_ladderSneakingLastFrame)
        {
            GetComponent<Rigidbody2D>().gravityScale = 0;
            _ladderSneakingLastFrame = true;
        }
        
        //Stopped ladder sneaking
        if (!isLadderSneaking && _ladderSneakingLastFrame)
        {
            GetComponent<Rigidbody2D>().gravityScale = 1;
            _ladderSneakingLastFrame = false;
        }
    }

    private void Awake()
    {
        _player = GetComponent<Player>();
        _movement = GetComponent<EntityMovement>();
    }
}

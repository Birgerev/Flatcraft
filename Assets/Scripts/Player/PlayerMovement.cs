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
    private bool _ladderSneaking;
    
    private Player _player;
    private EntityMovement _movement;

    private void Update()
    {
        if (!isOwned) return;
        
        PerformInput();
        CrouchOnLadderCheck();
    }

    [Client]
    private void PerformInput()
    {
        if (ChatMenu.instance.open || SignEditMenu.IsLocalMenuOpen()) return;
        if (Inventory.IsAnyOpen(_player.playerInstance)) return;
        
        //Toggle Debug disabled lighting
        if (Input.GetKeyDown(KeyCode.F4) && Debug.isDebugBuild)
        {
            LightManager.DoLight = !LightManager.DoLight;
            LightManager.UpdateAllLight();
        }

        //Open chat
        if (Input.GetKeyDown(KeyCode.T))
            ChatMenu.instance.open = true;
        
        
        //Walking
        if (Input.GetKey(KeyCode.A))
            _movement.Walk(-1);
        if (Input.GetKey(KeyCode.D))
            _movement.Walk(1);

        //Jumping
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.Space))
            _movement.Jump();

        //Sneaking
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.S))
        {
            SetServerSneaking(true);
            _movement.speed = SneakSpeed;
        }

        if (sneaking && (Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.S)))
        {
            SetServerSneaking(false);
            _movement.speed = _movement.walkSpeed;
        }

        //Sprinting
        if (sprinting && 
            (Input.GetKeyUp(KeyCode.LeftControl) || Mathf.Abs(_player.GetVelocity().x) < 0.5f || sneaking || _player.hunger <= 6))
        {
            SetServerSprinting(false);
            _movement.speed = _movement.walkSpeed;
        }
        
        if (Input.GetKeyDown(KeyCode.LeftControl) && _player.hunger > 6)
        {
            SetServerSprinting(true);
            _movement.speed = SprintSpeed;
        }

        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D))
        {
            if (Time.time - _lastDoubleTapSprintTap < 0.3f)
            {
                SetServerSprinting(true);
                _movement.speed = SprintSpeed;
            }
            
            _lastDoubleTapSprintTap = Time.time;
        }
    }

    [Command]
    private void SetServerSprinting(bool sprint)
    {
        sprinting = sprint;
    }
    [Command]
    private void SetServerSneaking(bool sneak)
    {
        sneaking = sneak;
    }

    public void CrouchOnLadderCheck()
    {
        bool isLadderSneakingThisFrame = _player.isOnClimbable && sneaking;
        if (isLadderSneakingThisFrame && !_ladderSneaking)
        {
            GetComponent<Rigidbody2D>().gravityScale = 0;
            _ladderSneaking = true;
        }
        else if (!isLadderSneakingThisFrame && _ladderSneaking)
        {
            GetComponent<Rigidbody2D>().gravityScale = 1;
            _ladderSneaking = false;
        }
    }

    private void Awake()
    {
        _player = GetComponent<Player>();
        _movement = GetComponent<EntityMovement>();
    }
}

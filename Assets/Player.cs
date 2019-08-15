using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : HumanEntity
{
    public override float maxHealth { get; } = 20;
    public float maxHunger = 20;
    public float hunger;

    [Header("Movement Properties")]
    public float walkSpeed;
    public float sprintSpeed;
    public float sneakSpeed;
    public float jumpVelocity;

    [Space]
    public Chunk currentChunk;
    public GameObject crosshair;

    public PlayerInventory inventory = new PlayerInventory();

    public static Player localInstance;

    public override void Start()
    {
        base.Start();

        localInstance = this;

        hunger = maxHunger;
    }

    public override void Update()
    {
        base.Update();

        performInput();


        if (getVelocity().magnitude > 0)
            currentChunk = Chunk.GetChunk((int)Mathf.Floor(transform.position.x / Chunk.Width));
        
        GetComponent<Rigidbody2D>().simulated = (currentChunk.isLoaded);            
    }

    private void performInput()
    {
        if (getVelocity().x < walkSpeed && getVelocity().x > -walkSpeed)
        {
            float targetXVelocity = 0;

            if (Input.GetKey(KeyCode.A))
                targetXVelocity -= walkSpeed;
            if (Input.GetKey(KeyCode.D))
                targetXVelocity += walkSpeed;

            //targetXVelocity *= Time.deltaTime;
            setVelocity(new Vector2(targetXVelocity, getVelocity().y));
        }

        if (Input.GetKey(KeyCode.W) && isOnGround)
            setVelocity(getVelocity() + new Vector2(0, jumpVelocity));

        //Crosshair
        Vector3Int mousePosition = Vector3Int.RoundToInt(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        mousePosition.z = 0;
        Block block = Chunk.getBlock((Vector2Int)mousePosition);

        crosshair.transform.position = mousePosition;

        if (block == null || (block.GetMateral() == Material.Water || block.GetMateral() == Material.Lava))
        {
            if (Input.GetMouseButtonDown(1))
            {
                if (inventory.getSelectedItem().material != Material.Air &&
                    (inventory.getSelectedItem().amount > 0))
                {

                    Chunk.setBlock((Vector2Int)mousePosition, inventory.getSelectedItem().material);
                    inventory.setItem(inventory.selectedSlot, 
new ItemStack(inventory.getSelectedItem().material, inventory.getSelectedItem().amount-1));
                }
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(1))
            {
                block.Interact();
            }
            if (Input.GetMouseButton(0))
            {
                block.Hit(Time.deltaTime);

            }
        }

        if (Input.GetKeyDown(KeyCode.Q))
            Drop();

        float scroll = Input.mouseScrollDelta.y;
        if (scroll != 0)
        {
            inventory.selectedSlot += ((int)scroll);
            inventory.selectedSlot %= 9;
            if (inventory.selectedSlot < 0)
                inventory.selectedSlot = 9 + inventory.selectedSlot;
        }

    }

    public void Drop()
    {
        inventory.getSelectedItem().Drop(Vector2Int.CeilToInt(transform.position + new Vector3(4, 0)));
        inventory.setItem(inventory.selectedSlot, new ItemStack());
    }
}

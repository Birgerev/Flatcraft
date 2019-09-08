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
        inventory = new PlayerInventory();
    }

    public override void Update()
    {
        base.Update();

        performInput();


        if (getVelocity().magnitude > 0)
            currentChunk = Chunk.GetChunk((int)Mathf.Floor(transform.position.x / Chunk.Width));

        if (WorldManager.instance.loadingProgress != 1)
            GetComponent<Rigidbody2D>().simulated = false;
        else
            GetComponent<Rigidbody2D>().simulated = (currentChunk.isLoaded);            
    }

    private void performInput()
    {
        if (getVelocity().x < walkSpeed && getVelocity().x > -walkSpeed)
        {
            float targetXVelocity = 0;

            if (Input.GetKey(KeyCode.A))
                targetXVelocity -= walkSpeed;
            else if (Input.GetKey(KeyCode.D))
                targetXVelocity += walkSpeed;
            else targetXVelocity = 0;

            //targetXVelocity *= Time.deltaTime;
            setVelocity(new Vector2(targetXVelocity, getVelocity().y));
        }

        if (Input.GetKey(KeyCode.W) && isOnGround)
            setVelocity(getVelocity() + new Vector2(0, jumpVelocity));

        //Crosshair
        if (WorldManager.instance.loadingProgress == 1)
        {
            Vector3Int mousePosition = Vector3Int.RoundToInt(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            mousePosition.z = 0;
            Block block = Chunk.getBlock((Vector2Int)mousePosition);

            crosshair.transform.position = mousePosition;


            if (System.Type.GetType(inventory.getSelectedItem().material.ToString()).IsSubclassOf(typeof(Block)))
            {
                if (block == null || (block.GetMateral() == Material.Water || block.GetMateral() == Material.Lava))
                {
                    if (Input.GetMouseButtonDown(1))
                    {
                        if (inventory.getSelectedItem().material != Material.Air &&
                            (inventory.getSelectedItem().amount > 0))
                        {

                            Chunk.setBlock((Vector2Int)mousePosition, inventory.getSelectedItem().material);
                            inventory.setItem(inventory.selectedSlot,
        new ItemStack(inventory.getSelectedItem().material, inventory.getSelectedItem().amount - 1));
                        }
                    }
                }
                else
                {
                    Item sampleItem = (Item)System.Activator.CreateInstance(typeof(Item));

                    if (Input.GetMouseButtonDown(1))
                    {
                        sampleItem.Interact((Vector2Int)mousePosition, 1, true);
                    }
                    else if (Input.GetMouseButton(1))
                    {
                        sampleItem.Interact((Vector2Int)mousePosition, 1, false);
                    }

                    if (Input.GetMouseButtonDown(0))
                    {
                        sampleItem.Interact((Vector2Int)mousePosition, 0, true);
                    }
                    else if (Input.GetMouseButton(0))
                    {
                        sampleItem.Interact((Vector2Int)mousePosition, 0, false);
                    }
                }
            }
            else if(System.Type.GetType(inventory.getSelectedItem().material.ToString()).IsSubclassOf(typeof(Item)))
            {
                Item item = (Item)System.Activator.CreateInstance(System.Type.GetType(inventory.getSelectedItem().material.ToString()));

                if (Input.GetMouseButtonDown(1))
                {
                    item.Interact((Vector2Int)mousePosition, 1, true);
                }
                else if (Input.GetMouseButton(1))
                {
                    item.Interact((Vector2Int)mousePosition, 1, false);
                }

                if (Input.GetMouseButtonDown(0))
                {
                    item.Interact((Vector2Int)mousePosition, 0, true);
                }
                else if (Input.GetMouseButton(0))
                {
                    item.Interact((Vector2Int)mousePosition, 0, false);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Q))
            Drop();

        float scroll = Input.mouseScrollDelta.y;
        if (scroll != 0)
        {
            inventory.selectedSlot -= ((int)scroll);
            inventory.selectedSlot %= 9;
            if (inventory.selectedSlot < 0)
                inventory.selectedSlot = 9 + inventory.selectedSlot;
        }

        if (Input.GetKeyDown(KeyCode.E))
            inventory.ToggleOpen();
    }

    public void Drop()
    {
        ItemStack item = inventory.getSelectedItem().Clone();

        if (item.amount <= 0)
            return;

        item.amount = 1;
        inventory.getSelectedItem().amount --;

        item.Drop(Vector2Int.CeilToInt(transform.position + new Vector3(4, 0)));
    }

    public void Spawn()
    {
        for (int i = Chunk.Height; i <= 0; i --)
        {
            if(Chunk.getBlock(Vector2Int.FloorToInt((Vector2)transform.position)) != null)
            {
                transform.position = new Vector3(transform.position.x, i+2);

                break;
            }
        }
    }

    public override void Die()
    {
        DeathMenu.active = true;

        base.Die();
    }
}

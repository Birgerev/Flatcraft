﻿using System.Collections.Generic;
using UnityEngine;

public class Portal_Frame : Block
{
    public override string texture { get; set; } = "block_portal_frame";
    public override bool solid { get; set; } = false;
    public override bool trigger { get; set; } = true;

    public override float breakTime { get; } = 9999999999f;

    public override Tool_Type propperToolType { get; } = Tool_Type.None;
    public override Block_SoundType blockSoundType { get; } = Block_SoundType.Stone;

    private Dictionary<Entity, float> entityTimeSpentInsidePortal = new Dictionary<Entity, float>();
    private float timeRequiredBeforeTeleport = 3f;

    public override ItemStack GetDrop()
    {
        return new ItemStack();
    }

    public override void Initialize()
    {
        base.Initialize();

        //Add random duration to teleportation time, so multiple portals wont teleport entity at the same time
        System.Random r = new System.Random(SeedGenerator.SeedByLocation(location));
        timeRequiredBeforeTeleport += ((float)r.NextDouble()*4);
    }

    public override void OnTriggerStay2D(Collider2D col)
    {
        Entity entity = col.GetComponent<Entity>();

        if (entity != null)
        {
            //Add time since last frame to table
            float timeSpentInPortal = 0;
            if (entityTimeSpentInsidePortal.ContainsKey(entity))
                timeSpentInPortal = entityTimeSpentInsidePortal[entity];

            entityTimeSpentInsidePortal[entity] = timeSpentInPortal + Time.deltaTime;


            //Teleport entity if time requirements are met
            if (timeSpentInPortal >= timeRequiredBeforeTeleport)
                PortalTeleport(entity);
        }

        base.OnTriggerStay2D(col);
    }

    public override void OnTriggerExit2D(Collider2D col)
    {
        Entity entity = col.GetComponent<Entity>();

        if (entity != null)
        {
            //Reset time spent in portal if entity leaves portal
            entityTimeSpentInsidePortal.Remove(entity);
        }

        base.OnTriggerExit2D(col);
    }

    private void PortalTeleport(Entity entity)
    {
        Location entityLocation = entity.Location;
        Dimension currentDimension = entityLocation.dimension;
        Location newLocation = new Location(0, 0);

        if (currentDimension == Dimension.Overworld)
        {
            newLocation = new Location(entityLocation.x / 8, entityLocation.y, Dimension.Nether);
        }
        else if (currentDimension == Dimension.Nether)
        {
            newLocation = new Location(entityLocation.x * 8, entityLocation.y, Dimension.Overworld);
        }

        entity.Location = newLocation;
    }
}
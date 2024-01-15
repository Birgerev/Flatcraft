using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jukebox : Block
{
    public override bool IsSolid { get; set; } = false;
    public override float BreakTime { get; } = 6;

    public override Tool_Type ProperToolType { get; } = Tool_Type.Axe;
    public override BlockSoundType BlockSoundType { get; } = BlockSoundType.Wood;

    public override void Interact(PlayerInstance player)
    {
        base.Interact(player);

        if (GetData().GetTag("stored_disc").Length != 0)
        {
            EjectStoredDisc();
            return;
        }  
        TryInsertDisc(player);
    }

    private void EjectStoredDisc()
    {
        string discMaterialName = GetData().GetTag("stored_disc");
        Material material = (Material) Enum.Parse(typeof(Material), discMaterialName);
        ItemStack item = new ItemStack(material);
        
        item.Drop(location + new Location(0, 1), true);
        location.SetData(GetData().RemoveTag("stored_disc"));

        Sound.DestroySounds(location, 1);
    }
    
    private void TryInsertDisc(PlayerInstance player)
    {
        Player playerEntity = player.playerEntity;
        PlayerInventory inv = playerEntity.GetInventoryHandler().GetInventory();
        
        ItemStack heldDisc = inv.GetSelectedItem();

        if (heldDisc.material != Material.Music_Disc_Stal)
            return;
        
        //Save disc as stored
        location.SetData(GetData().SetTag("stored_disc", heldDisc.material.ToString()));
            
        //Remove one disc from player inventory
        heldDisc.Amount--;
        inv.SetItem(inv.selectedSlot, heldDisc);
        ChatManager.instance.AddMessagePlayer("Now playing: C418 - Stal", player);

        Sound.Play(location, "music/disc/stal/stal", SoundType.Music, 1, 1, 32);
    }
}

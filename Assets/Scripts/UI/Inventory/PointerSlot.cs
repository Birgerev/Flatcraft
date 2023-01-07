using UnityEngine;

public class PointerSlot : ItemSlot
{
    private void Start()
    {
        item = new ItemStack();
    }

    private void Update()
    {
        //always update pointer (overriding previous if statements)
        UpdateSlotContents();
        Tooltip.isPointerHoldingItem = item.material != Material.Air;
    }

    // Update is called once per frame
    public override void UpdateSlotContents()
    {
        base.UpdateSlotContents();

        //Move Pointer item to mouse position
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        
        transform.position = mousePos;
    }
}
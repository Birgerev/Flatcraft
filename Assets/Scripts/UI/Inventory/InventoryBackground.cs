using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryBackground : MonoBehaviour
{
    public void Click()
    {
        GetComponentInParent<InventoryMenu>().CMD_OnClickBackground();
    }
}

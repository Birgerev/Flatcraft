using UnityEngine.UI;
using UnityEngine;

public class Tooltip : MonoBehaviour
{
    public Text tooltipTitle;
    
    public static ItemStack hoveredItem = new ItemStack();
    public static bool isPointerHoldingItem;

    private CanvasGroup canvasGroup;
    
    // Update is called once per frame
    void Update()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        canvasGroup.alpha = (hoveredItem.material == Material.Air || isPointerHoldingItem) ? 0 : 1;
        tooltipTitle.text = hoveredItem.material.ToString();
    }
}

using UnityEngine;
using UnityEngine.UI;

public class Tooltip : MonoBehaviour
{
    public static ItemStack hoveredItem = new ItemStack();
    public static bool isPointerHoldingItem;
    public Text tooltipTitle;

    private CanvasGroup canvasGroup;

    // Update is called once per frame
    private void Update()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        canvasGroup.alpha = hoveredItem.material == Material.Air || isPointerHoldingItem ? 0 : 1;
        tooltipTitle.text = hoveredItem.material.ToString();
    }
}
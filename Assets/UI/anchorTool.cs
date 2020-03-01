using UnityEngine;
using System.Collections;
[ExecuteInEditMode]
public class anchorTool : MonoBehaviour
{
    public bool manualRefresh = true;
    public Rect anchorRect;
    public Vector2 anchorVector;
    private Rect anchorRectOld;
    private Vector2 anchorVectorOld;
    private RectTransform ownRectTransform;
    private RectTransform parentRectTransform;
    private Vector2 pivotOld;
    private Vector2 offsetMinOld;
    private Vector2 offsetMaxOld;

    void Update()
    {
#if UNITY_EDITOR
        ownRectTransform = gameObject.GetComponent<RectTransform>();
        parentRectTransform = transform.parent.gameObject.GetComponent<RectTransform>();
        if (ownRectTransform.offsetMin != offsetMinOld || ownRectTransform.offsetMax != offsetMaxOld)
        {
            CalculateCurrentWH();
            CalculateCurrentXY();
        }
        if (ownRectTransform.pivot != pivotOld || anchorVector != anchorVectorOld)
        {
            CalculateCurrentXY();
            pivotOld = ownRectTransform.pivot;
            anchorVectorOld = anchorVector;
        }
        if (anchorRect != anchorRectOld)
        {
            AnchorsToCorners();
            anchorRectOld = anchorRect;
        }
        if (manualRefresh)
        {
            manualRefresh = false;
            CalculateCurrentWH();
            CalculateCurrentXY();
            AnchorsToCorners();
        }
#endif
    }

    public void StopDrag()
    {
        CalculateCurrentWH();
        CalculateCurrentXY();
        AnchorsToCorners();
    }

    private void CalculateCurrentXY()
    {
        float pivotX = anchorRect.width * ownRectTransform.pivot.x;
        float pivotY = anchorRect.height * (1 - ownRectTransform.pivot.y);
        Vector2 newXY = new Vector2(ownRectTransform.anchorMin.x * parentRectTransform.rect.width + ownRectTransform.offsetMin.x + pivotX - parentRectTransform.rect.width * anchorVector.x,
                                  -(1 - ownRectTransform.anchorMax.y) * parentRectTransform.rect.height + ownRectTransform.offsetMax.y - pivotY + parentRectTransform.rect.height * (1 - anchorVector.y));
        anchorRect.x = newXY.x;
        anchorRect.y = newXY.y;
        anchorRectOld = anchorRect;
    }

    private void CalculateCurrentWH()
    {
        anchorRect.width = ownRectTransform.rect.width;
        anchorRect.height = ownRectTransform.rect.height;
        anchorRectOld = anchorRect;
    }

    private void AnchorsToCorners()
    {
        float pivotX = anchorRect.width * ownRectTransform.pivot.x;
        float pivotY = anchorRect.height * (1 - ownRectTransform.pivot.y);
        ownRectTransform.anchorMin = new Vector2(0f, 1f);
        ownRectTransform.anchorMax = new Vector2(0f, 1f);
        ownRectTransform.offsetMin = new Vector2(anchorRect.x / ownRectTransform.localScale.x, anchorRect.y / ownRectTransform.localScale.y - anchorRect.height);
        ownRectTransform.offsetMax = new Vector2(anchorRect.x / ownRectTransform.localScale.x + anchorRect.width, anchorRect.y / ownRectTransform.localScale.y);
        ownRectTransform.anchorMin = new Vector2(ownRectTransform.anchorMin.x + anchorVector.x + (ownRectTransform.offsetMin.x - pivotX) / parentRectTransform.rect.width * ownRectTransform.localScale.x,
                                                 ownRectTransform.anchorMin.y - (1 - anchorVector.y) + (ownRectTransform.offsetMin.y + pivotY) / parentRectTransform.rect.height * ownRectTransform.localScale.y);
        ownRectTransform.anchorMax = new Vector2(ownRectTransform.anchorMax.x + anchorVector.x + (ownRectTransform.offsetMax.x - pivotX) / parentRectTransform.rect.width * ownRectTransform.localScale.x,
                                                 ownRectTransform.anchorMax.y - (1 - anchorVector.y) + (ownRectTransform.offsetMax.y + pivotY) / parentRectTransform.rect.height * ownRectTransform.localScale.y);
        ownRectTransform.offsetMin = new Vector2((0 - ownRectTransform.pivot.x) * anchorRect.width * (1 - ownRectTransform.localScale.x), (0 - ownRectTransform.pivot.y) * anchorRect.height * (1 - ownRectTransform.localScale.y));
        ownRectTransform.offsetMax = new Vector2((1 - ownRectTransform.pivot.x) * anchorRect.width * (1 - ownRectTransform.localScale.x), (1 - ownRectTransform.pivot.y) * anchorRect.height * (1 - ownRectTransform.localScale.y));

        offsetMinOld = ownRectTransform.offsetMin;
        offsetMaxOld = ownRectTransform.offsetMax;
    }
}

//X and Y set the position of the Pivot relative to the parent Rect
//Anchor X and Y set where on the parent Rect the Pivot is relative to
//Where (0, 0) is the bottom left corner of parent Rect and (1, 1) the top right
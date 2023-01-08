using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasRenderCameraAssigner : MonoBehaviour
{
    private Canvas _canvas;

    // Start is called before the first frame update
    void Start()
    {
        _canvas = GetComponent<Canvas>();
        if (_canvas == null)
        {
            Debug.LogError(this.name + " could not find Canvas component on object '" + gameObject.name + "', destructing...");
            Destroy(this);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_canvas.worldCamera == null)
        {
            _canvas.worldCamera = Camera.main;
            _canvas.sortingLayerName = "UI";
        }
    }
}

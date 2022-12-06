using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SignCursor : MonoBehaviour
{
    public SignPreview preview;
    public GameObject canvas;
    
    // Update is called once per frame
    void Update()
    {
        if (Player.localEntity != null)
        {
            //Fetch block data from hovered block
            BlockState state = Player.localEntity.GetBlockedMouseLocation().GetState();

            //Show if block is sign
            if (state.material == Material.Oak_Sign)
            {
                //Move to mouse
                Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                transform.position = mousePos;

                //Apply sign lines to preview sign
                for (int i = 0; i < preview.lines.Length; i++)
                    preview.lines[i] = state.data.GetTag("line" + i);

                //Show
                canvas.SetActive(true);
                return;
            }
        }
        //Hide
        canvas.SetActive(false);
    }
}

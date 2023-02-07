using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureExistsTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        foreach(Material mat in Enum.GetValues(typeof(Material))) 
        {  
            if(new ItemStack(mat).GetSprite() == null)
                Debug.LogError("Texture missing for material " + mat);
        } 
    }
}

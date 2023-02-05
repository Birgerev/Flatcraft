using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderValueText : MonoBehaviour
{
    public Slider slider;
    public string prefix;
    public string suffix;
    
    
    private Text _text;
    
    // Start is called before the first frame update
    void Start()
    {
        _text = GetComponent<Text>();
        
        if(_text == null)
            Debug.LogError("No text was found");
    }

    // Update is called once per frame
    void Update()
    {
        _text.text = prefix + slider.value + suffix;
    }
}

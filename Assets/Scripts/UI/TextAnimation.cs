using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextAnimation : MonoBehaviour
{
    public String[] textFrames;
    public float tickSpeed;
    
    private int _currentIndex = 0;
    private Text _text;
    
    // Start is called before the first frame update
    void Start()
    {
        _text = GetComponent<Text>();
        
        InvokeRepeating(nameof(Tick), 0, tickSpeed);
    }

    private void Tick()
    {
        if (_currentIndex >= textFrames.Length)
            _currentIndex = 0;
        
        _text.text = textFrames[_currentIndex];

        _currentIndex++;
    }
}

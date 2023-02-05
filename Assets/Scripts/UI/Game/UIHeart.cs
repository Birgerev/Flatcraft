using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class UIHeart : MonoBehaviour
{
    private const float BlinkDuration = 0.1f;
    
    public Sprite empty;
    public Sprite full;
    public Sprite half;
    public Sprite blink;

    private float _lastFrameHealth; 
    private float _lastBlinkTime;
    
    // Update is called once per frame
    private void Update()
    {
        if (Player.localEntity == null)
            return;

        CalculateBlink();
        CalculateSprite();
        
        _lastFrameHealth = Player.localEntity.health;
    }

    private void CalculateSprite()
    {
        float playerHearts = Player.localEntity.health / 2;
        int heartIndex = transform.GetSiblingIndex();
        float heartPortion = playerHearts - heartIndex;

        //Test for empty
        if (heartPortion <= 0)
        {
            GetComponent<Image>().sprite = empty;
            return;
        }

        //If were still in blink period, show blink
        if (Time.time - _lastBlinkTime < BlinkDuration)
        {
            GetComponent<Image>().sprite = blink;
            return;
        }
        
        //Test for half
        if (heartPortion > 0)
            GetComponent<Image>().sprite = half;
        
        //Test for full
        if (heartPortion > .5f)
            GetComponent<Image>().sprite = full;
    }
    
    private void CalculateBlink()
    {
        if (Player.localEntity.health < _lastFrameHealth)
            _lastBlinkTime = Time.time;
    }
}
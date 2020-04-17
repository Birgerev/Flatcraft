using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakIndicator : MonoBehaviour
{
    public static BreakIndicator instance;

    private float lastTimeChanged = 0f;

    private void Start()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time > lastTimeChanged + 0.5f) {
            Hide();
            return;
        }
        if(Chunk.getBlock(Location.locationByPosition(transform.position, Player.localInstance.location.dimension)) == null)
        {
            Hide();
            return;
        }
    }

    public void Hide()
    {
        transform.position = Vector3.zero;
        GetComponent<SpriteRenderer>().sprite = null;
    }

    public void UpdateState(int index, Location loc)
    {
        Sprite[] sprites = Resources.LoadAll<Sprite>("Sprites/Block_Break");

        Sprite sprite = null;
        if(index < sprites.Length && index >= 0)
            sprite = sprites[index];

        transform.position = loc.getPosition();
        GetComponent<SpriteRenderer>().sprite = sprite;

        lastTimeChanged = Time.time;
    }
}

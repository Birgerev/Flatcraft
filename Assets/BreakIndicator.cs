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
        if(Chunk.getBlock(Vector2Int.RoundToInt(transform.position)) == null)
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

    public void UpdateState(int index, Vector2Int pos)
    {
        Sprite[] sprites = Resources.LoadAll<Sprite>("Sprites/Block_Break");

        transform.position = (Vector2)pos;
        GetComponent<SpriteRenderer>().sprite = sprites[index];

        lastTimeChanged = Time.time;
    }
}

using UnityEngine;

public class BreakIndicator : MonoBehaviour
{
    public static BreakIndicator instance;

    private float lastTimeChanged;

    private void Start()
    {
        instance = this;
    }

    // Update is called once per frame
    private void Update()
    {
        if (Time.time > lastTimeChanged + 0.5f)
        {
            Hide();
            return;
        }

        if (Location.LocationByPosition(transform.position, Player.localInstance.location.dimension).GetBlock() ==
            null) Hide();
    }

    public void Hide()
    {
        transform.position = Vector3.zero;
        GetComponent<SpriteRenderer>().sprite = null;
    }

    public void UpdateState(int index, Location loc)
    {
        var sprites = Resources.LoadAll<Sprite>("Sprites/Block_Break");

        Sprite sprite = null;
        if (index < sprites.Length && index >= 0)
            sprite = sprites[index];

        transform.position = loc.GetPosition();
        GetComponent<SpriteRenderer>().sprite = sprite;

        lastTimeChanged = Time.time;
    }
}
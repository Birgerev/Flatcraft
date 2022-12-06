using UnityEngine;
using UnityEngine.UI;

public class UIHeart : MonoBehaviour
{
    public Sprite empty;
    public Sprite full;
    public Sprite half;

    // Start is called before the first frame update
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
        float health;


        if (Player.localEntity == null)
            health = 0;
        else
            health = Mathf.Round((Player.localEntity.health + 0.70f) * 2) / 2;

        float heartIndex = transform.GetSiblingIndex();

        GetComponent<Image>().sprite = full;

        if ((int) health / 2 <= heartIndex)
            GetComponent<Image>().sprite = empty;

        if ((int) health % 2 == 1 && (int) health / 2 == heartIndex)
            GetComponent<Image>().sprite = half;
    }
}
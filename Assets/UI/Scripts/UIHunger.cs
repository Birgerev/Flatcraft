using UnityEngine;
using UnityEngine.UI;

public class UIHunger : MonoBehaviour
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
        if (Player.localEntity == null)
            return;

        int hunger = Mathf.CeilToInt(Player.localEntity.hunger * 2) / 2;
        float hungerIndex = transform.GetSiblingIndex();

        GetComponent<Image>().sprite = full;

        if (hunger / 2 <= hungerIndex)
            GetComponent<Image>().sprite = empty;

        if (hunger % 2 == 1 && hunger / 2 == hungerIndex)
            GetComponent<Image>().sprite = half;
    }
}
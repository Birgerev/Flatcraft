using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class UIHeart : MonoBehaviour
{
    public Sprite full;
    public Sprite half;
    public Sprite empty;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float health;


        if (Player.localInstance == null)
            health = 0;
        else
            health = Mathf.Round(Player.localInstance.health * 2) / 2;

        float heartIndex = transform.GetSiblingIndex();

        GetComponent<Image>().sprite = full;

        if ((int)health / 2 <= heartIndex)
            GetComponent<Image>().sprite = empty;

        if ((int)health % 2 == 1 && (int)health / 2 == heartIndex)
            GetComponent<Image>().sprite = half;
    }
}

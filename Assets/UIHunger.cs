using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class UIHunger : MonoBehaviour
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
        if (Player.localInstance == null)
            return;

        float hunger = Mathf.Round(Player.localInstance.hunger*2)/2;
        float hungerIndex = transform.GetSiblingIndex();

        GetComponent<Image>().sprite = full;

        if ((int)hunger/2 <= hungerIndex)
            GetComponent<Image>().sprite = empty;

        if ((int)hunger % 2 == 1 && (int)hunger / 2 == hungerIndex)
            GetComponent<Image>().sprite = half;

    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class StyledText : MonoBehaviour
{
    public Text shadowText;
    public Color shadowTextColor;
    public Vector3 shadowTextOffset;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(shadowText == null)
        {
            GameObject obj = Instantiate(gameObject);

            obj.transform.SetParent(transform);
            obj.transform.localScale = new Vector3(1, 1, 1);
            obj.transform.localPosition = shadowTextOffset;
            shadowText = obj.GetComponent<Text>();
            shadowText.color = transform.GetComponent<Text>().color;
            transform.GetComponent<Text>().color = shadowTextColor;
            Destroy(shadowText.GetComponent<StyledText>());
        }
    }
}

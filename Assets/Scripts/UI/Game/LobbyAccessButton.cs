using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class LobbyAccessButton : MonoBehaviour
{
    private Button _button;
    
    // Start is called before the first frame update
    void Awake()
    {
        _button = GetComponent<Button>();
    }

    // Update is called once per frame
    void Update()
    {
        _button.interactable = NetworkServer.active;
    }
}

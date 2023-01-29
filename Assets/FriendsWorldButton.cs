using System.Collections;
using System.Collections.Generic;
using Steamworks;
using Steamworks.Data;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class FriendsWorldButton : MonoBehaviour
{
    private const float DoubleClickMaxTime = 0.3f;
    public Lobby lobby;
    
    public Text titleText;
    public Text descriptionText;
    public Text playerCountText;
    
    private Button _button;
    private MultiplayerMenu _menuManager;
    private float _lastClickTime;
    
    void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(Click);
    }
    
    private void Start()
    {
        _menuManager = GetComponentInParent<MultiplayerMenu>();
    }

    private void Update()
    {
        if (_menuManager.selectedLobby.Equals(lobby))
            _button.Select();   //Make the UI button view as selected

        titleText.text = lobby.Owner.Name + "'s World";
        descriptionText.text = "Join Friend's World";
        playerCountText.text = lobby.MemberCount.ToString();
    }

    public void Click()
    {
        _menuManager.selectedLobby = lobby;

        if (Time.time - _lastClickTime < DoubleClickMaxTime)
            _menuManager.Play();

        _lastClickTime = Time.time;
    }
}

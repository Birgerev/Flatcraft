using UnityEngine;

#if !DISABLESTEAMWORKS
using Steamworks;
#endif

public class FriendsWorldButton : MonoBehaviour
{
#if !DISABLESTEAMWORKS
    private const float DoubleClickMaxTime = 0.3f;
    public CSteamID lobbyId;
    public CSteamID friendId;
    
    public Text titleText;
    public Text descriptionText;
    
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
        if (!SteamManager.Initialized)
            return;
        
        if (_menuManager.selectedLobby.Equals(lobbyId))
            _button.Select();   //Make the UI button view as selected

        titleText.text = SteamFriends.GetFriendPersonaName(friendId) + "'s World";
        descriptionText.text = "Join Friend's World";
    }

    public void Click()
    {
        _menuManager.selectedLobby = lobbyId;

        if (Time.time - _lastClickTime < DoubleClickMaxTime)
            _menuManager.Play();

        _lastClickTime = Time.time;
    }
#endif
}

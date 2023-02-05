using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DedicatedServerButton : MonoBehaviour
{
    public DedicatedServer server;
    
    public float lastClickTime;
    public Text nameText;
    public Text descriptionText;
    public Text ownerText;
    public Text playerCountText;
    public Button button;
    private OldMultiplayerMenu menuManager;

    private void Start()
    {
        menuManager = GetComponentInParent<OldMultiplayerMenu>();
    }

    private void Update()
    {
        if (GetComponentInParent<CanvasGroup>().alpha == 0)
            return;
        
        if (menuManager.selectedServer.Equals(server))
            button.Select();

        nameText.text = server.name;
        descriptionText.text = server.description;
        ownerText.text = "Hosted by " + server.owner;
        playerCountText.text = server.playerCount + "/" + server.maxPlayerCount;
    }

    public void Click()
    {
        GetComponentInParent<OldMultiplayerMenu>().selectedServer = server;

        if (Time.time - lastClickTime < 0.3f)
            GetComponentInParent<OldMultiplayerMenu>().Play();

        lastClickTime = Time.time;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class SignEditMenu : NetworkBehaviour
{
    public CanvasGroup canvasGroup;
    public SignPreview signPreview;
    public InputField inputField;
    
    [SyncVar] public GameObject ownerPlayerInstance;
    [SyncVar] public Location signLocation;
    public string[] newSignLines = new string[4];
    public int selectedLine = 0;
        
    public void Start()
    {
        WorldManager.instance.openSignMenus.Add(this);

        BlockData data = signLocation.GetData();
        for(int i = 0; i < 4; i++)
        {
            newSignLines[i] = data.GetTag("line" + i);
        }
        
        inputField.text = newSignLines[selectedLine];
    }

    public void OnDestroy()
    {
        WorldManager.instance.openSignMenus.Remove(this);
    }

    public static bool IsLocalMenuOpen()
    {
        foreach (SignEditMenu menu in WorldManager.instance.openSignMenus)
        {
            if(menu.ownerPlayerInstance == PlayerInstance.localPlayerInstance.gameObject)
                return true;
        }

        return false;
    }

    public virtual void Update()
    {
        bool ownsInventoryMenu = (PlayerInstance.localPlayerInstance != null &&
                                 PlayerInstance.localPlayerInstance.gameObject == ownerPlayerInstance);

        canvasGroup.alpha = ownsInventoryMenu ? 1 : 0;
        canvasGroup.interactable = ownsInventoryMenu;
        canvasGroup.blocksRaycasts = ownsInventoryMenu;

        if (ownsInventoryMenu)
        {
            UserUpdate();
        }
    }

    private void UserUpdate()
    {
        //Check if close menu input
        if (Input.GetKeyDown(KeyCode.Escape))
            CloseButton();

        //Check if line change input occured
        int lineChange = 0;
        if (Input.GetKeyDown(KeyCode.UpArrow))
            lineChange = -1;
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.Return))
            lineChange = 1;

        //Handle line change
        if (lineChange != 0)
        {
            selectedLine = Mathf.Clamp(selectedLine + lineChange, 0, 3);
            inputField.text = newSignLines[selectedLine];
        }
        
        //Make sure input field is being written to
        inputField.Select();
        inputField.ActivateInputField();
        //Disable selection
        inputField.caretPosition = inputField.text.Length;
        //Update selected line to input field text
        newSignLines[selectedLine] = inputField.text;
        
        //Update Sign preview
        signPreview.lines = (string[])newSignLines.Clone();
        //Place caret on selected line
        signPreview.lines[selectedLine] += (Time.time % 1 < 0.5f) ? "_" : "  ";
    }

    public void CloseButton()
    {
        SetSignText(newSignLines);
        Close();
    }
    
    [Command(requiresAuthority = false)]
    public void Close()
    {
        NetworkServer.Destroy(gameObject);
    }
    
    [Command(requiresAuthority = false)]
    public void SetSignText(string[] lines)
    {
        BlockData data = signLocation.GetData();
        
        for (int i = 0; i < lines.Length; i++)
        {
            data.SetTag("line" + i, lines[i]);
        }

        signLocation.SetData(data);
    }

    [Server]
    public static void Create(PlayerInstance player, Location sign)
    {
        GameObject obj = Instantiate(Resources.Load<GameObject>("Prefabs/SignEditMenu"));
        SignEditMenu menu = obj.GetComponent<SignEditMenu>();

        menu.ownerPlayerInstance = player.gameObject;
        menu.signLocation = sign;
        
        NetworkServer.Spawn(obj);
    }
}

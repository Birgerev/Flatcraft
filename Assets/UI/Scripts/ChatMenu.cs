using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatMenu : MonoBehaviour
{
    public static ChatMenu instance;

    public GameObject chatEntryPrefab;
    public Transform chatEntryList;
    
    public InputField inputField;
    public bool open;
    
    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        inputField.gameObject.SetActive(open);
        
        if (!open)
        {
            inputField.text = "";
            return;
        }
            
        inputField.Select();
        inputField.ActivateInputField();

        if(Input.GetKeyDown(KeyCode.Escape))
            open = false;
        
        if (Input.GetKeyDown(KeyCode.Return))
        {
            open = false;
            
            string text = inputField.text;

            if (!String.IsNullOrWhiteSpace(text))
            {
                ChatManager.instance.RequestSendMessage(inputField.text);
            }
        }
    }

    public void AddMessage(string text)
    {
        GameObject obj = Instantiate(chatEntryPrefab, chatEntryList);
        ChatEntry entry = obj.GetComponent<ChatEntry>();
        entry.message = text;
    }
}

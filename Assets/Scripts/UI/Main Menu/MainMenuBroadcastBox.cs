using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class MainMenuBroadcastBox : MonoBehaviour
{
    private const string Url = "http://hille.evansson.se/flatcraft/broadcast.html";
    public Text text;
    private CanvasGroup thisGroup;
    
    // Start is called before the first frame update
    void Start()
    {
        thisGroup = GetComponent<CanvasGroup>();
        StartCoroutine(fetchBroadcastMessage());
    }

    IEnumerator fetchBroadcastMessage()
    {
        Debug.Log("Fetching Broadcast Message");
        UnityWebRequest www = UnityWebRequest.Get(Url);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.LogError("Fetching Broadcast Message failed: " + www.error);
            yield break;
        }

        string message = www.downloadHandler.text;
        
        if (String.IsNullOrWhiteSpace(message))
        {
            Debug.Log("No Broadcast Message was found");
            yield break;
        }

        thisGroup.alpha = 1;
        text.text = message;
        Debug.Log("Showing broadcast message: " + message);
    }
}

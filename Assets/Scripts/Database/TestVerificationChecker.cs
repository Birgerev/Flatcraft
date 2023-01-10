using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class TestVerificationChecker : MonoBehaviour
{
    public bool doVersionCheck;
    
    private const string Url = "http://hille.evansson.se/flatcraft/testingVerification.html";
    private const float LoopDuration = 10;
    
    
    // Start is called before the first frame update
    void Start()
    {
        if(doVersionCheck)
            StartCoroutine(verificationLoop());
    }

    IEnumerator verificationLoop()
    {
        while (true)
        {
            UnityWebRequest www = UnityWebRequest.Get(Url);
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.LogError(www.error);
                VerificationTestFailed();
            }
            else
            {
                int verificationId = Int32.Parse(www.downloadHandler.text);
                
                if (verificationId != Version.currentId)
                {
                    VerificationTestFailed();
                }
            }

            yield return new WaitForSeconds(LoopDuration);
        }
    }

    private void VerificationTestFailed()
    {
        Debug.Log("Build verification failed");
        SceneManager.LoadScene("BuildVerificationFail");
        Destroy(gameObject);
    }
}

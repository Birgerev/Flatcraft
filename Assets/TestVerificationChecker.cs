using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class TestVerificationChecker : MonoBehaviour
{
    private const string Url = "http://79.142.222.17/flatcraft/testingVerification.html";  //TODO static ip
    private const float LoopDuration = 10;
    
    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject);
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
                string verificationCode = www.downloadHandler.text;
                
                print(verificationCode);
                print(VersionController.GetVersionName());
                if (!verificationCode.Equals(VersionController.GetVersionName()))
                {
                    VerificationTestFailed();
                }
            }

            yield return new WaitForSeconds(LoopDuration);
        }
    }

    private void VerificationTestFailed()
    {
        SceneManager.LoadScene("BuildVerificationFail");
        Destroy(gameObject);
    }
}

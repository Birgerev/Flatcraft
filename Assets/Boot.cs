using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class Boot : MonoBehaviour
{
    void Start()
    {
        SceneManager.LoadScene("MainMenu");
    }
}

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SetNameMenu : MonoBehaviour
{
    public InputField nameField;

    public void DoneButton()
    {
        string testingNamePath = Application.dataPath + "/../testingProfile.dat";
        File.WriteAllText(testingNamePath, nameField.text);
        
        SceneManager.LoadScene("Boot");
    }
}

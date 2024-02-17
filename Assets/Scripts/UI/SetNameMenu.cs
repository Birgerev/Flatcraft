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
        SetName(nameField.text);
        SceneManager.LoadScene("Boot");
    }

    private void SetName(string name)
    {
        string testingNamePath = Application.persistentDataPath + "\\playerProfile.dat";
        File.WriteAllText(testingNamePath, name);
    }
}

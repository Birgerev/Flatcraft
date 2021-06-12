using System;
using System.Net;
using UnityEditor;
using UnityEngine;

namespace ParrelSync.Update
{
    /// <summary>
    ///     A simple update checker
    /// </summary>
    public class UpdateChecker
    {
        private const string LocalVersionFilePath = "Assets/ParrelSync/VERSION.txt";

        [MenuItem("ParrelSync/Check for update", priority = 20)]
        private static void CheckForUpdate()
        {
            using (WebClient client = new WebClient())
            {
                try
                {
                    string localVersionText = AssetDatabase.LoadAssetAtPath<TextAsset>(LocalVersionFilePath).text;
                    Debug.Log("Local version text : " + localVersionText);

                    string latesteVersionText = client.DownloadString(ExternalLinks.RemoteVersionURL);
                    Debug.Log("latest version text got: " + latesteVersionText);
                    string messageBody = "Current Version: " + localVersionText + "\n"
                                         + "Latest Version: " + latesteVersionText + "\n";
                    Version latestVersion = new Version(latesteVersionText);
                    Version localVersion = new Version(localVersionText);

                    if (latestVersion > localVersion)
                    {
                        Debug.Log("There's a newer version");
                        messageBody += "There's a newer version available";
                        if (EditorUtility.DisplayDialog("Check for update.", messageBody, "Get latest release"
                            , "Close"))
                            Application.OpenURL(ExternalLinks.Releases);
                    }
                    else
                    {
                        Debug.Log("Current version is up-to-date.");
                        messageBody += "Current version is up-to-date.";
                        EditorUtility.DisplayDialog("Check for update.", messageBody, "OK");
                    }
                }
                catch (Exception exp)
                {
                    Debug.LogError("Error with checking update. Exception: " + exp);
                    EditorUtility.DisplayDialog("Update Error"
                        , "Error with checking update. \nSee console fore more details.",
                        "OK");
                }
            }
        }
    }
}
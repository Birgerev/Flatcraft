using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VersionController : MonoBehaviour
{
    public static int CurrentVersionId
    {
        get
        {
            return versionNames.Count - 1;
        }
    }
    public static List<string> versionNames = new List<string> { 
        "Indev 9", "Indev 10", "Indev 11", "Indev 12" };
    
    public static string GetVersionName()
    {
        return GetVersionName(CurrentVersionId);
    }

    public static string GetVersionName(int versionId)
    {
        if (versionId >= versionNames.Count)
            return "Future Version";
        
        return versionNames[versionId];
    }
}

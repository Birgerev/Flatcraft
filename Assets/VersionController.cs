using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VersionController : MonoBehaviour
{
    public static int CurrentVersionId = 0;
    public static List<string> versionNames = new List<string> { 
        "Indev 9", "Indev 10" };
    
    public static string GetVersionName()
    {
        return GetVersionName(CurrentVersionId);
    }

    public static string GetVersionName(int versionId)
    {
        return versionNames[versionId];
    }
}

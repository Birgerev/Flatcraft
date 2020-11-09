using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VersionController : MonoBehaviour
{
    public static int CurrentVersionId = 0;
    public static List<string> versionNames = new List<string> { "indev 9" };
    
    public static string GetVersionName()
    {
        return GetVersionName(CurrentVersionId);
    }

    public static string GetVersionName(int versionId)
    {
        return versionNames[versionId];
    }
}

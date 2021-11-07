using System.Collections.Generic;
using UnityEngine;

public class VersionController : MonoBehaviour
{
    public static List<string> versionNames = new List<string>
    {
        "Indev 9", "Indev 10", "Indev 11", "Indev 12", "Indev 13", "Indev 14 / simon test 1", "Indev 14 / simon test 2", 
        "Alpha 1.0 Candidate 1", "Alpha 1.0 Demo", "Alpha 1.0"
    };

    public static int CurrentVersionId => versionNames.Count - 1;

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
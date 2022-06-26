using System.Collections.Generic;
using UnityEngine;

public class Version : MonoBehaviour
{
    public readonly static List<string> versions = new List<string>
    {
        "Indev 9", "Indev 10", "Indev 11", "Indev 12", "Indev 13", "Indev 14 / simon test 1", "Indev 14 / simon test 2", 
        "Alpha 1.0 Candidate 1", "Alpha 1.0 Demo", "Alpha 1.0", "Alpha 1.1 Candidate 1"
    };

    public static int currentId => versions.Count - 1;

    public static string CurrentName()
    {
        return NameOf(currentId);
    }

    public static string NameOf(int versionId)
    {
        if (versionId >= versions.Count)
            return "Future Version";

        return versions[versionId];
    }
}
using System.Collections.Generic;
using UnityEngine;

public class Version : MonoBehaviour
{
    public readonly static List<string> versions = new List<string>
    {
        "Indev 9", "Indev 10", "Indev 11", "Indev 12", "Indev 13", "Indev 14 / simon test 1", "Indev 14 / simon test 2", 
        "Alpha 1.0 Candidate 1", "Alpha 1.0 Demo", "Alpha 1.0", "Alpha 1.1 Candidate 1", "Alpha 1.2", "Alpha 1.3", 
        "Alpha 1.4 Demo", "Alpha 1.4", "Alpha 1.4.1", "Alpha 1.4.2", "Alpha 1.4.2 Demo", "Alpha 1.5", "Alpha 1.6",
        "Beta 1.0", "Beta 1.0.1", "Beta 1.0.2", "Beta 1.0.3", "Beta 1.0.4", "Beta 1.0.4", "Beta 1.1.0 Demo", "Beta 1.1.0 Demo", 
        "1.0"
    };
    //Reminder to change project version!

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
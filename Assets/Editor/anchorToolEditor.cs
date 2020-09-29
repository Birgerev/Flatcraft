using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(anchorTool))]
internal class anchorToolEditor : Editor
{
    private void OnSceneGUI()
    {
        if (Event.current.type == EventType.MouseUp && Event.current.button == 0)
        {
            var myTarget = (anchorTool) target;
            myTarget.StopDrag();
        }
    }
}

//This script must be placed in a folder called "Editor" in the root of the "Assets"
//Otherwise the script will not work as intended
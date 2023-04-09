using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Mathematics;
using UnityEngine;

public class TransposeDebugStructure : MonoBehaviour
{
    public string structureId = "Debug";
    public int xOffset;
    public int yOffset;
    
    // Start is called before the first frame update
    void Start()
    {
        string savePath = Application.dataPath + "/Resources/Structure/"+structureId+".txt";
        List<string> newLines = new List<string>();
        
        foreach (String line in File.ReadAllLines(savePath))
        {
            string[] lineParts = line.Split('*');

            if (lineParts.Length < 3)
                continue;
            
            //Material
            string material = lineParts[0];
            
            //Location
            string locationString = lineParts[1];
            int2 loc = new int2(
                int.Parse(locationString.Split(',')[0]),
                int.Parse(locationString.Split(',')[1])
            );
            
            //Data
            string data = lineParts[2];

            //Offset
            loc += new int2(xOffset, yOffset);
                        
            newLines.Add(material + "*" + loc.x + "," + loc.y + "*" + data);
        }
                    
        File.WriteAllLines(savePath, newLines);
        Debug.Log("Offset structure '" + structureId + "' offset with "+xOffset+","+yOffset);
    }
}

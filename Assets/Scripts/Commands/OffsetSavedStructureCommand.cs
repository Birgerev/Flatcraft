using System;
using System.Collections.Generic;
using System.IO;
using Unity.Mathematics;
using UnityEngine;

public class OffsetSavedStructureCommand : Command
{
    public OffsetSavedStructureCommand()
    {
        name = "offsetsavedstructure";
        parameters = new string[] {"<X Offset>", "<Y Offset>"};
    }

    public override void Execute(string[] param, PlayerInstance player)
    {
        base.Execute(param, player);
        
        try
        {
            if (param.Length != 2)
            {
                ChatManager.instance.AddMessage("Usage: " + UsageGuide());
                return;
            }
            int xOffset = int.Parse(param[0]);
            int yOffset = int.Parse(param[1]);
            
            string savePath = Application.persistentDataPath + "\\savedStructure.txt";
            List<string> newLines = new List<string>();
                
            foreach (String line in File.ReadAllLines(savePath))
            {
                string mat = line.Split('*')[0];
                string locString = line.Split('*')[1];
                int2 loc = new int2(
                    int.Parse(locString.Split(',')[0]),
                    int.Parse(locString.Split(',')[1])
                );
                string data = line.Split('*')[2];

                loc += new int2(xOffset, yOffset);
                    
                newLines.Add(mat + "*" + loc.x + "," + loc.y + "*" + data);
            }
                
            File.WriteAllLines(savePath, newLines);
            ChatManager.instance.AddMessage("Structure saved to: " + savePath);
        }
        catch (Exception e){ SendCommandError(e.StackTrace); }
    }
}
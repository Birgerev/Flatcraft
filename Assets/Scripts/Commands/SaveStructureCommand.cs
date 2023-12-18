using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveStructureCommand : Command
{
    public SaveStructureCommand()
    {
        name = "savestructure";
        parameters = new string[] {"<Include Air>", "<Width>", "<Height>"};
    }

    public override void Execute(string[] param, PlayerInstance player)
    {
        base.Execute(param, player);
        
        try
        {
            if (param.Length != 3)
            {
                ChatManager.instance.AddMessage("Usage: " + UsageGuide());
                return;
            }
            
            bool includeAir = param[0].ToLower().Equals("true");
            int width = int.Parse(param[1]);
            int height = int.Parse(param[2]);
            
            Location playerLocation = player.playerEntity.Location;
            string savePath = Application.persistentDataPath + "\\savedStructure.txt";
            List<string> lines = new List<string>();

            ChatManager.instance.AddMessage("Starting Structure Cloner");
            for (int localX = 0; localX < width; localX++)
            {
                for (int localY = 0; localY < height; localY++)
                {
                    Location worldLocation = playerLocation + new Location(localX, localY);
                    Material mat = worldLocation.GetMaterial();
                    BlockData data = worldLocation.GetData();
                        
                    if(mat == Material.Air && !includeAir)
                        continue;
                        
                    lines.Add(mat.ToString() + "*" + localX + "," + localY + "*" + data.ToString());
                }
            }
                
            File.WriteAllLines(savePath, lines);
            ChatManager.instance.AddMessage("Structure saved to: " + savePath);
        }
        catch (Exception e){ SendCommandError(e.StackTrace); }
    }
}
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Structure_Block : Block
{
    public static string default_texture  = "block_structure";
    public override float breakTime { get; } = 10000000;

    public string tickResult;

    public override void Tick(bool spread)
    {
        if (data.ContainsKey("structure"))
            tickResult = data["structure"];
        else
            tickResult = "empty";

        if (data.ContainsKey("structure"))
        {
            string structureId = data["structure"];
            TextAsset[] structures = Resources.LoadAll<TextAsset>("Structure/" + structureId);
            TextAsset structure = structures[new System.Random(Chunk.seedByPosition(position)).Next(0, structures.Length)];
            bool save = (data["save"] == "false") ? false : true;

            Material replaceMaterial = Material.Air;
            string replaceData = "";

            foreach (string blockText in structure.text.Split(new char[] { '\n', '\r'}))
            {
                if (blockText.Length < 4)
                    continue;
                Material mat = (Material)System.Enum.Parse(typeof(Material), blockText.Split('*')[0]);
                Vector2Int pos = new Vector2Int(
                    int.Parse(blockText.Split('*')[1].Split(',')[0]),
                    int.Parse(blockText.Split('*')[1].Split(',')[1]));
                string data = blockText.Split('*')[2];

                if (pos == Vector2Int.zero)
                {
                    replaceMaterial = mat;
                    replaceData = data;
                    continue;
                }

                pos += position;

                Chunk.setBlock(pos, mat, data, save, false);
            }
            
            Chunk.setBlock(position, replaceMaterial, replaceData, save, false);

            base.Tick(spread);
        }
    }
}

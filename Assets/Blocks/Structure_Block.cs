using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Structure_Block : Block
{
    public override string default_texture { get; } = "block_structure";
    public override float breakTime { get; } = 10000000;

    public override void Tick()
    {
        base.Tick();

        if (data.ContainsKey("structure"))
        {
            string structureId = data["structure"];
            TextAsset[] structures = Resources.LoadAll<TextAsset>("Structure/" + structureId);
            TextAsset structure = structures[new System.Random(Chunk.seedByPosition(getPosition())).Next(0, structures.Length)];
            bool save = (data["save"] == "false") ? false : true;

            bool currentBlockHasBeenReplaced = false;

            foreach(string blockText in structure.text.Split(new char[] { '\n', '\r'}))
            {
                if (blockText.Length < 4)
                    continue;
                Material mat = (Material)System.Enum.Parse(typeof(Material), blockText.Split('*')[0]);
                Vector2Int pos = new Vector2Int(
                    int.Parse(blockText.Split('*')[1].Split(',')[0]),
                    int.Parse(blockText.Split('*')[1].Split(',')[1]));
                string data = blockText.Split('*')[2];

                if (pos == Vector2Int.zero)
                    currentBlockHasBeenReplaced = true;

                pos += getPosition();

                Chunk.setBlock(pos, mat, save);
            }

            if (currentBlockHasBeenReplaced)
                Destroy(gameObject);
            else
                Chunk.setBlock(getPosition(), Material.Air, false);
        }
    }
}

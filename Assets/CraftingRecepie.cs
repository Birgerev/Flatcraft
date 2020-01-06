using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CraftingRecepie
{
    public ItemStack result;
    public Dictionary<Vector2Int, Material> recepieShape = new Dictionary<Vector2Int, Material>();

    public CraftingRecepie()
    {

    }

    public bool Compare(ItemStack[] items)
    {
        Dictionary<Vector2Int, Material> shape = new Dictionary<Vector2Int, Material>();

        //Get shape by item list
        if (items.Length == 4)
        {
            for (int i = 0; i < 4; i++)
            {
                Vector2Int curPos = Vector2Int.zero;
                switch (i)
                {
                    case 0:
                        curPos = new Vector2Int(0, 1);
                        break;
                    case 1:
                        curPos = new Vector2Int(1, 1);
                        break;
                    case 2:
                        curPos = new Vector2Int(0, 0);
                        break;
                    case 3:
                        curPos = new Vector2Int(1, 0);
                        break;
                }
                if (items[i].material != Material.Air)
                {
                    shape.Add(curPos, items[i].material);
                }
            }
        }
        else if (items.Length == 9)
        {
            for (int i = 0; i < 9; i++)
            {
                Vector2Int curPos = Vector2Int.zero;
                switch (i) {
                    case 0:
                        curPos = new Vector2Int(0, 2);
                        break;
                    case 1:
                        curPos = new Vector2Int(1, 2);
                        break;
                    case 2:
                        curPos = new Vector2Int(2, 2);
                        break;
                    case 3:
                        curPos = new Vector2Int(0, 1);
                        break;
                    case 4:
                        curPos = new Vector2Int(1, 1);
                        break;
                    case 5:
                        curPos = new Vector2Int(2, 1);
                        break;
                    case 6:
                        curPos = new Vector2Int(0, 0);
                        break;
                    case 7:
                        curPos = new Vector2Int(1, 0);
                        curPos = new Vector2Int(1, 0);
                        break;
                    case 8:
                        curPos = new Vector2Int(2, 0);
                        break;
                }

                if (items[i] != null && items[i].material != Material.Air)
                {
                    shape.Add(curPos, items[i].material);
                }
            }
        } else Debug.LogError("Invalid Crating Table Size Of " + items.Length);
        
        //Find How much the recepie is Offsetted by
        int lowest_x = 2;
        int lowest_y = 2;

        foreach (KeyValuePair<Vector2Int, Material> shapeItem in shape)
        {
            if(shapeItem.Key.x < lowest_x)
            {
                lowest_x = shapeItem.Key.x;
            }
            if (shapeItem.Key.y < lowest_y)
            {
                lowest_y = shapeItem.Key.y;
            }
        }

        Vector2Int[] keys = shape.Keys.ToArray<Vector2Int>();
        Dictionary<Vector2Int, Material> shapeCopy = new Dictionary <Vector2Int, Material> (shape);
        shape.Clear();

        //Move To Lowest Slots
        foreach (Vector2Int key in keys)
        {
            Material mat = shapeCopy[key];

            shape.Add(key - new Vector2Int(lowest_x, lowest_y), mat);
        }

        //Compare both Dictionaries
        bool anyDifferences = false;

        if (recepieShape.Count != shape.Count)
            anyDifferences = true;

        foreach (KeyValuePair<Vector2Int, Material> recepieItem in recepieShape)
        {
            if(!shape.ContainsKey(recepieItem.Key))
            {
                anyDifferences = true;
                break;
            }
            if(shape[recepieItem.Key] != recepieItem.Value)
            {
                anyDifferences = true;
                break;
            }
        }

        return !anyDifferences;
    }

    public static CraftingRecepie FindRecepieByItems(ItemStack[] items)
    {
        foreach (CraftingRecepie recepie in allRecepies())
        {
            if (recepie.Compare(items))
            {
                return recepie;
            }
        }
        return null;
    }

    public static CraftingRecepie[] allRecepies()
    {
        TextAsset[] files = Resources.LoadAll<TextAsset>("Recepies/Crafting");
        List<CraftingRecepie> recepies = new List<CraftingRecepie>();

        foreach(TextAsset file in files)
        {
            recepies.Add(FileToRecepie(file));
        }

        return recepies.ToArray();
    }

    public static CraftingRecepie FileToRecepie(TextAsset file)
    {
        CraftingRecepie recepie = new CraftingRecepie();

        try
        {
            string[] lines = file.text.Split('\n');

            recepie.result = new ItemStack(
                (Material)System.Enum.Parse(typeof(Material), lines[0].Split('*')[0]),
                int.Parse(lines[0].Split('*')[1]), lines[0].Split('*')[2]);

            for (int i = 1; i < lines.Length; i++)
            {
                Material mat = (Material)System.Enum.Parse(typeof(Material), lines[i].Split('*')[0]);
                Vector2Int pos = new Vector2Int(
                    int.Parse(lines[i].Split('*')[1].Split(',')[0]),
                    int.Parse(lines[i].Split('*')[1].Split(',')[1]));

                recepie.recepieShape.Add(pos, mat);
            }
        }catch(System.Exception e)
        {
            Debug.LogError("faulty crafting recepie \""+file.name+"\", error: "+e.StackTrace);
        }

        return recepie;
    }
}

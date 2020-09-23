using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockData
{
    public Dictionary<string, string> dataTable = new Dictionary<string, string>();
    
    public BlockData()
    {
    }

    public BlockData(Dictionary<string, string> dataTable)
    {
        this.dataTable = dataTable;
    }
    
    public BlockData(string saveDataString)
    {
        foreach (string dataPiece in saveDataString.Split('|'))
        {
            if(dataPiece.Contains("="))
                dataTable.Add(dataPiece.Split('=')[0], dataPiece.Split('=')[1]);
        }
    }
    
    public void RemoveData(string key)
    {
        dataTable.Remove(key);
    }
    
    public void SetData(string key, string value)
    {
        dataTable[key] = value;
    }
    
    public bool HasData(string key)
    {
        return dataTable.ContainsKey(key);
    }
    
    public string GetData(string key)
    {
        if (!HasData(key))
            return "";
        
        return dataTable[key];
    }

    public string GetSaveString()
    {
        string result = "";

        bool first = true;
        foreach (KeyValuePair<string, string> entry in dataTable)
        {
            if (!first)
                result += "|";
            result += entry.Key + "=" + entry.Value; 
            first = false;
        }

        return result;
    }
}

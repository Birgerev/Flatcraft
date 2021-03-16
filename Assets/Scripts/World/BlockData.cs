using System.Collections.Generic;

public class BlockData
{
    public List<string> keys = new List<string>();
    public List<string> values = new List<string>();

    public BlockData()
    {
    }

    public BlockData(string saveDataString)
    {
        foreach (var tagStrings in saveDataString.Split('|'))
            if (tagStrings.Contains("="))
                SetTag(tagStrings.Split('=')[0], tagStrings.Split('=')[1]);
    }

    public void RemoveTag(string key)
    {
        if (HasTag(key))
        {
            int index = keys.IndexOf(key);
            
            keys.RemoveAt(index);
            values.RemoveAt(index);
        }
    }

    public BlockData SetTag(string key, string value)
    {
        if (HasTag(key))
        {
            values[keys.IndexOf(key)] = value;
        }
        else
        {
            keys.Add(key);
            values.Add(value);
        }
        
        return this;
    }

    public bool HasTag(string key)
    {
        return (keys.Contains(key));
    }

    public string GetTag(string key)
    {
        if (!HasTag(key))
            return "";

        return values[keys.IndexOf(key)];
    }

    public string GetSaveString()
    {
        var result = "";

        for (int i = 0; i < keys.Count; i++)
        {
            if(i != 0)
                result += "|";
            
            result += keys[i] + "=" + values[i];
        }

        return result;
    }
}
using System.Collections.Generic;

public struct BlockData
{
    public List<string> keys;
    public List<string> values;

    public BlockData(BlockData cloneData)
    {
        keys = new List<string>(cloneData.keys);
        values = new List<string>(cloneData.values);
    }

    public BlockData(string saveDataString)
    {
        keys = new List<string>();
        values = new List<string>();

        foreach (string tagStrings in saveDataString.Split('|'))
            if (tagStrings.Contains("="))
                SetTag(tagStrings.Split('=')[0], tagStrings.Split('=')[1]);
    }

    public BlockData RemoveTag(string key)
    {
        if (HasTag(key))
        {
            int index = keys.IndexOf(key);

            keys.RemoveAt(index);
            values.RemoveAt(index);
        }

        return this;
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
        return keys.Contains(key);
    }

    public string GetTag(string key)
    {
        if (!HasTag(key))
            return "";

        return values[keys.IndexOf(key)];
    }

    public override string ToString()
    {
        string result = "";

        for (int i = 0; i < keys.Count; i++)
        {
            if (i != 0)
                result += "|";

            result += keys[i] + "=" + values[i];
        }

        return result;
    }
}
using UnityEngine;

public class Command
{
    public string name;
    public string[] parameters;
    
    public Command()
    {
    }

    public virtual void Execute(string[] param, PlayerInstance player)
    {
    }

    public string UsageGuide()
    {
        string result = "/" + name + " ";

        foreach (var parameter in parameters)
        {
            result = result + " " + parameter;
        }
        
        return result;
    }

    public void SendCommandError(string stackTrace)
    {
        ChatManager.instance.AddMessage("/" + name + " command failed");
        Debug.LogError("chat error: " + stackTrace);
    }
}

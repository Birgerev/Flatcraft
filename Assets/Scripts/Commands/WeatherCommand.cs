using System;

public class WeatherCommand : Command
{
    public WeatherCommand()
    {
        name = "weather";
        parameters = new string[] {""};
    }

    public override void Execute(string[] param, PlayerInstance player)
    {
        base.Execute(param, player);
        
        try
        {
            WeatherManager.instance.ChangeWeather();
            ChatManager.instance.AddMessage("Toggled weather");
        }
        catch (Exception e){ SendCommandError(e.StackTrace); }
    }
}
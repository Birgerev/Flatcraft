using System;

[AttributeUsage(AttributeTargets.All)]
public class EntityDataTag : Attribute
{
    public readonly bool json;

    public EntityDataTag(bool json)
    {
        this.json = json;
    }
}

using System;

[AttributeUsage(AttributeTargets.Field)]
public class EntitySaveField : Attribute
{
    public readonly bool ConvertToJson;

    public EntitySaveField(bool convertToJson)
    {
        ConvertToJson = convertToJson;
    }
}
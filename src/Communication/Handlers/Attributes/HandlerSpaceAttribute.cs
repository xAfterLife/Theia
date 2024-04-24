namespace Communication.Handlers.Attributes;

public class HandlerSpaceAttribute(string spaceName) : Attribute
{
    public string SpaceName { get; } = spaceName;
}
namespace Communication.Handlers;

public class HandlerSpaceAttribute(string spaceName) : Attribute
{
    public string SpaceName { get; } = spaceName;
}
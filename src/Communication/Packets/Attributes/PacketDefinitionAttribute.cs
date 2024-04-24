namespace Communication.Packets.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class PacketDefinitionAttribute(string header, string? handlerSpace) : Attribute
{
    public string Header { get; } = header;
    public string? HandlerSpace { get; } = handlerSpace;
}
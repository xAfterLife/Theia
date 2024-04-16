namespace Communication.Packets;

/// <summary>
///  Attribute to Identify Packets and Instantiate their Handlers
/// </summary>
/// <param name="header"></param>
public class PacketDefinitionAttribute(string header, string space) : Attribute
{
    public string Header { get; private set; } = header;
    public string Space { get; private set; } = space;
}
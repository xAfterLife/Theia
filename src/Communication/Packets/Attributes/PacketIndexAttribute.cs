namespace Communication.Packets.Attributes;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class PacketIndexAttribute(int index) : Attribute
{
    /// <summary>
    ///     Zero based index of packet content starting from the header (exclusive)
    /// </summary>
    public int Index { get; } = index;
}
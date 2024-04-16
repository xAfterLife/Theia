namespace Communication.Packets;

[PacketDefinition("login", "auth")]
public sealed class LoginPacket : IPacket
{
    [PacketIndex(0)]
    public required string GameVersion { get; set; }

    [PacketIndex(1)]
    public required string Username { get; set; }

    [PacketIndex(2)]
    public required string Password { get; set; }
}
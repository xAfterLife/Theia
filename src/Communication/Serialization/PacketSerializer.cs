using System.Buffers;
using Communication.Packets;
using MessagePack;

namespace Communication.Serialization;

public static class PacketSerializer
{
    public static byte[] SerializePacket(Packet packet)
    {
        var packetBytes = MessagePackSerializer.Serialize(packet, SerializationConfiguration.Options);
        var sizeBytes = BitConverter.GetBytes(packetBytes.Length);

        var returnArray = new byte[packetBytes.Length + sizeBytes.Length];
        Buffer.BlockCopy(sizeBytes, 0, returnArray, 0, sizeBytes.Length);
        Buffer.BlockCopy(packetBytes, 0, returnArray, sizeBytes.Length, packetBytes.Length);

        return returnArray;
    }

    public static IEnumerable<Packet> DeserializePackets(byte[] message)
    {
        var offset = 0;

        while ( offset < message.Length )
        {
            var packetLength = BitConverter.ToInt32(message, offset);
            offset += sizeof(int);

            var packetBytes = new ReadOnlySequence<byte>(message, offset, packetLength);
            offset += packetLength;

            yield return MessagePackSerializer.Deserialize<Packet>(packetBytes, SerializationConfiguration.Options);
        }
    }
}
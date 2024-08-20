using System.Buffers;
using Communication.Packets;
using MemoryPack;

namespace Communication.Serialization;

public static class PacketSerializer
{
    private static readonly object Lock = new();

    public static byte[] SerializePacket(Packet packet)
    {
        var packetBytes = MemoryPackSerializer.Serialize(packet);
        var sizeBytes = BitConverter.GetBytes(packetBytes.Length);

        var returnArray = new byte[packetBytes.Length + sizeBytes.Length];
        Buffer.BlockCopy(sizeBytes, 0, returnArray, 0, sizeBytes.Length);
        Buffer.BlockCopy(packetBytes, 0, returnArray, sizeBytes.Length, packetBytes.Length);

        return returnArray;
    }

    public static List<Packet> DeserializePackets(byte[] message, ref byte[] incompletePacketBuffer)
    {
        lock ( Lock )
        {
            if ( incompletePacketBuffer is { Length: > 0 } )
            {
                var newBuffer = new byte[incompletePacketBuffer.Length + message.Length];
                Buffer.BlockCopy(incompletePacketBuffer, 0, newBuffer, 0, incompletePacketBuffer.Length);
                Buffer.BlockCopy(message, 0, newBuffer, incompletePacketBuffer.Length, message.Length);
                message = newBuffer;
                incompletePacketBuffer = [];
            }
        }

        var offset = 0;
        var packets = new List<Packet>();

        while ( offset < message.Length )
        {
            if ( offset + sizeof(int) > message.Length )
            {
                incompletePacketBuffer = message.AsSpan(offset).ToArray();
                break;
            }

            var packetLength = BitConverter.ToInt32(message.AsSpan(offset, sizeof(int)));

            if ( offset + sizeof(int) + packetLength > message.Length )
            {
                incompletePacketBuffer = message.AsSpan(offset).ToArray();
                break;
            }

            offset += sizeof(int);
            var packetBytes = new ReadOnlySequence<byte>(message, offset, packetLength);
            offset += packetLength;

            packets.Add(MemoryPackSerializer.Deserialize<Packet>(packetBytes));
        }

        return packets;
    }
}
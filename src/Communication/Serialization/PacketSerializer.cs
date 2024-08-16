using System.Buffers;
using Communication.Packets;
using MessagePack;

namespace Communication.Serialization;

public sealed class PacketSerializer
{
    private readonly object _lock = new();
    private byte[]? _incompletePacketBuffer;

    public byte[] SerializePacket(Packet packet)
    {
        var packetBytes = MessagePackSerializer.Serialize(packet, SerializationConfiguration.Options);
        var sizeBytes = BitConverter.GetBytes(packetBytes.Length);

        var returnArray = new byte[packetBytes.Length + sizeBytes.Length];
        Buffer.BlockCopy(sizeBytes, 0, returnArray, 0, sizeBytes.Length);
        Buffer.BlockCopy(packetBytes, 0, returnArray, sizeBytes.Length, packetBytes.Length);

        return returnArray;
    }

    public IEnumerable<Packet> DeserializePackets(byte[] message)
    {
        lock ( _lock )
        {
            if ( _incompletePacketBuffer is { Length: > 0 } )
            {
                var newBuffer = new byte[_incompletePacketBuffer.Length + message.Length];
                Buffer.BlockCopy(_incompletePacketBuffer, 0, newBuffer, 0, _incompletePacketBuffer.Length);
                Buffer.BlockCopy(message, 0, newBuffer, _incompletePacketBuffer.Length, message.Length);
                message = newBuffer;
                _incompletePacketBuffer = null;
            }
        }

        var offset = 0;

        while ( offset < message.Length )
        {
            if ( offset + sizeof(int) > message.Length )
            {
                _incompletePacketBuffer = message.AsSpan(offset).ToArray();
                break;
            }

            var packetLength = BitConverter.ToInt32(message.AsSpan(offset, sizeof(int)));

            if ( offset + sizeof(int) + packetLength > message.Length )
            {
                _incompletePacketBuffer = message.AsSpan(offset).ToArray();
                break;
            }

            offset += sizeof(int);
            var packetBytes = new ReadOnlySequence<byte>(message, offset, packetLength);
            offset += packetLength;

            yield return MessagePackSerializer.Deserialize<Packet>(packetBytes, SerializationConfiguration.Options);
        }
    }
}
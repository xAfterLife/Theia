using MessagePack;

namespace Communication.Serialization;

public static class SerializationConfiguration
{
    public static readonly MessagePackSerializerOptions Options = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);
}
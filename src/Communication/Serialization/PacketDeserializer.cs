using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using Communication.Packets;
using Communication.Packets.Attributes;

namespace Communication.Serialization;

public static class PacketDeserializer
{
    private static readonly ConcurrentDictionary<string, (Type Type, Dictionary<int, Action<IPacket, string>> Setters)> Packets;

    static PacketDeserializer()
    {
        Packets = new ConcurrentDictionary<string, (Type, Dictionary<int, Action<IPacket, string>> Setters)>();
        LoadPacketTypes();
    }

    private static void LoadPacketTypes()
    {
        var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()).Where(IsPacket);

        foreach ( var type in types )
        {
            var attribute = type.GetCustomAttribute<PacketDefinitionAttribute>();
            if ( attribute == null )
                continue;

            var setters = new Dictionary<int, Action<IPacket, string>>();
            var properties = type.GetProperties();

            foreach ( var property in properties )
            {
                var indexAttribute = property.GetCustomAttribute<PacketIndexAttribute>();
                if ( indexAttribute == null )
                    continue;

                var instance = Expression.Parameter(typeof(IPacket), "instance");
                var value = Expression.Parameter(typeof(string), "value");

                var propertySetter = Expression.Lambda<Action<IPacket, string>>(Expression.Assign(Expression.Property(Expression.Convert(instance, type), property), Expression.Convert(Expression.Call(typeof(Convert), nameof(Convert.ChangeType), null, value, Expression.Constant(property.PropertyType)), property.PropertyType)), instance, value).Compile();

                setters[indexAttribute.Index] = propertySetter;
            }

            Packets.TryAdd(attribute.Header, (type, setters));
        }
    }

    private static bool IsPacket(Type type)
    {
        return typeof(IPacket).IsAssignableFrom(type) && type.GetCustomAttribute<PacketDefinitionAttribute>() != null;
    }

    public static IPacket? DeserializePacket(Packet packet)
    {
        if ( !Packets.TryGetValue(packet.Header, out var packetTuple) )
            return null;

        var packetInstance = (IPacket?)Activator.CreateInstance(packetTuple.Type);
        if ( packetInstance == null )
            return null;

        var packetData = packet.Content.Split(PacketConfiguration.ContentDelimiter);

        foreach ( var (index, setter) in packetTuple.Setters )
        {
            if ( index >= packetData.Length )
                return null;

            var value = packetData[index];
            setter(packetInstance, value);
        }

        return packetInstance;
    }
}
using System.Collections.Concurrent;
using System.Reflection;
using Communication.Packets;
using Communication.Packets.Attributes;

namespace Benchmarking;

internal static class Reflections
{
    private static readonly ConcurrentDictionary<string, Type> PacketTypes = new();
    private static readonly ConcurrentDictionary<Type, Dictionary<PacketIndexAttribute, PropertyInfo>> PacketProperties = new();

    static Reflections()
    {
        LoadPacketTypes();
    }

    private static void LoadPacketTypes()
    {
        var packetIndexPropertyInfos = new Dictionary<PacketIndexAttribute, PropertyInfo>();
        var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()).Where(IsPacket);

        foreach ( var type in types )
        {
            var attribute = type.GetCustomAttribute<PacketDefinitionAttribute>();
            if ( attribute == null )
                continue;

            GetIndexPropertyInfos(type, packetIndexPropertyInfos);

            PacketTypes.TryAdd(attribute.Header, type);
            PacketProperties.TryAdd(type, new Dictionary<PacketIndexAttribute, PropertyInfo>(packetIndexPropertyInfos));

            packetIndexPropertyInfos.Clear();
        }
    }

    private static void GetIndexPropertyInfos(Type type, Dictionary<PacketIndexAttribute, PropertyInfo> packetIndexPropertyInfos)
    {
        foreach ( var property in type.GetProperties() )
        {
            var att = property.GetCustomAttribute<PacketIndexAttribute>();
            if ( att != null )
                packetIndexPropertyInfos[att] = property;
        }
    }

    private static bool IsPacket(Type type)
    {
        return typeof(IPacket).IsAssignableFrom(type) && type.GetCustomAttribute<PacketDefinitionAttribute>() != null;
    }

    public static IPacket? DeserializePacket(Packet packet)
    {
        if ( !PacketTypes.TryGetValue(packet.Header, out var type) )
            return null;

        if ( !PacketProperties.TryGetValue(type, out var packetProperties) )
            return null;

        var packetInstance = (IPacket?)Activator.CreateInstance(type);
        if ( packetInstance == null )
            return null;

        var packetData = packet.Content.Split(PacketConfiguration.ContentDelimiter);

        foreach ( var (attr, property) in packetProperties )
        {
            if ( attr.Index >= packetData.Length )
                return null;

            var value = packetData[attr.Index];

            var convertedValue = Convert.ChangeType(value, property.PropertyType);
            property.SetValue(packetInstance, convertedValue);
        }

        return packetInstance;
    }
}
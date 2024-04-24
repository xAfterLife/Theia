using System.Reflection;
using Communication.Packets.Attributes;

namespace Communication.Packets;

public class PacketDeserializer
{
    public static readonly Dictionary<string, Type> PacketTypes = [];
    public static readonly Dictionary<Type, Dictionary<PacketIndexAttribute, PropertyInfo>> PacketProperties = [];

    static PacketDeserializer()
    {
        LoadPacketTypes();
    }

    public static void LoadPacketTypes()
    {
        Dictionary<PacketIndexAttribute, PropertyInfo> packetIndexPropertyInfos = [];
        var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes());

        foreach ( var type in types.Where(x => typeof(IPacket).IsAssignableFrom(x) && x.GetCustomAttribute<PacketDefinitionAttribute>() != null) )
        {
            var attribute = type.GetCustomAttribute<PacketDefinitionAttribute>();
            if ( attribute == null )
                continue;

            PacketTypes[attribute.Header] = type;

            foreach ( var x in type.GetProperties() )
            {
                var att = x.GetCustomAttribute<PacketIndexAttribute>();
                if ( att != null )
                    packetIndexPropertyInfos.Add(att, x);
            }

            PacketProperties[type] = packetIndexPropertyInfos;
        }
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
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using Communication.Packets;
using Communication.Packets.Attributes;

namespace Benchmarking;

internal static class Expressions
{
    private static readonly ConcurrentDictionary<string, Type> PacketTypes = new();
    private static readonly ConcurrentDictionary<Type, Dictionary<int, Action<IPacket, string>>> PacketSetters = new();
    private static readonly ConcurrentDictionary<Type, Func<IPacket>> PacketCreators = new();

    static Expressions()
    {
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

                var convertMethod = typeof(Convert).GetMethod("ChangeType", [typeof(object), typeof(Type)]);
                var convertedValue = Expression.Convert(Expression.Call(convertMethod, value, Expression.Constant(property.PropertyType)), property.PropertyType);
                var propertySetter = Expression.Lambda<Action<IPacket, string>>(Expression.Assign(Expression.Property(Expression.Convert(instance, type), property), convertedValue), instance, value).Compile();

                setters[indexAttribute.Index] = propertySetter;
            }

            PacketTypes.TryAdd(attribute.Header, type);
            PacketSetters.TryAdd(type, setters);

            // Cache instance creation
            var newExpression = Expression.New(type);
            var lambda = Expression.Lambda<Func<IPacket>>(newExpression);
            PacketCreators.TryAdd(type, lambda.Compile());
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

        if ( !PacketSetters.TryGetValue(type, out var setters) )
            return null;

        if ( !PacketCreators.TryGetValue(type, out var creator) )
            return null;

        var packetInstance = creator();
        var packetData = packet.Content.Split(PacketConfiguration.ContentDelimiter);

        foreach ( var (index, setter) in setters )
        {
            if ( index >= packetData.Length )
                return null;

            var value = packetData[index];
            setter(packetInstance, value);
        }

        return packetInstance;
    }
}
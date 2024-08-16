using System.Collections.Concurrent;
using System.Reflection;
using Communication.Handlers.Attributes;

namespace Communication.Handlers;

public static class PacketProcessorFactory
{
    private static readonly ConcurrentDictionary<string, Dictionary<Type, IPacketHandler>> HandlerBySpaces = new();

    static PacketProcessorFactory()
    {
        LoadHandlers();
    }

    public static PacketProcessor CreateProcessor(string handlerSpace)
    {
        if ( !HandlerBySpaces.TryGetValue(handlerSpace, out var packetHandlers) )
            packetHandlers ??= new Dictionary<Type, IPacketHandler>();

        return new PacketProcessor(handlerSpace, packetHandlers);
    }

    private static void LoadHandlers()
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach ( var assembly in assemblies )
        {
            var types = assembly.GetTypes();

            foreach ( var type in types.Where(IsPacketHandler) )
                AddOrUpdateTypeHandler(type);
        }
    }

    private static void AddOrUpdateTypeHandler(Type type)
    {
        var packetType = GetPacketType(type);
        var handlerSpace = type.GetCustomAttribute<HandlerSpaceAttribute>()?.SpaceName;

        if ( string.IsNullOrWhiteSpace(handlerSpace) || packetType == null || Activator.CreateInstance(type) is not IPacketHandler handler )
            return;

        HandlerBySpaces.AddOrUpdate(handlerSpace, new Dictionary<Type, IPacketHandler> { { packetType, handler } }, (_, dict) =>
        {
            dict[packetType] = handler;
            return dict;
        });
    }

    private static bool IsPacketHandler(Type type)
    {
        return type.BaseType is
        {
            IsGenericType: true
        } && type.BaseType.GetGenericTypeDefinition() == typeof(PacketHandler<>);
    }

    private static Type? GetPacketType(Type handlerType)
    {
        return handlerType.BaseType?.GetGenericArguments()[0];
    }
}
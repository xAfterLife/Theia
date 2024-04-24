using System.Reflection;
using Communication.Handlers.Attributes;

namespace Communication.Handlers;

public static class PacketProcessorFactory
{
    private static Dictionary<string, Dictionary<Type, IPacketHandler>>? _handlerBySpaces;

    static PacketProcessorFactory()
    {
        LoadHandlers();
    }

    public static PacketProcessor CreateProcessor(string handlerSpace)
    {
        if ( _handlerBySpaces == null )
            return new PacketProcessor(handlerSpace, new Dictionary<Type, IPacketHandler>());

        if ( !_handlerBySpaces.TryGetValue(handlerSpace, out var packetHandlers) )
            packetHandlers ??= new Dictionary<Type, IPacketHandler>();

        return new PacketProcessor(handlerSpace, packetHandlers);
    }

    private static void LoadHandlers()
    {
        _handlerBySpaces = new Dictionary<string, Dictionary<Type, IPacketHandler>>();

        foreach ( var assemblyName in Assembly.GetEntryAssembly()?.GetReferencedAssemblies()! )
        {
            var assembly = Assembly.Load(assemblyName);
            var types = assembly.GetTypes();

            foreach ( var type in types.Where(IsPacketHandler) )
            {
                var handlerSpace = type.GetCustomAttribute<HandlerSpaceAttribute>()?.SpaceName;
                if ( string.IsNullOrWhiteSpace(handlerSpace) )
                    continue;

                var packetType = GetPacketType(type);
                if ( packetType == null )
                    continue;

                if ( Activator.CreateInstance(type) as IPacketHandler is not {} handler )
                    continue;

                if ( !_handlerBySpaces.TryGetValue(handlerSpace, out var dict) )
                {
                    dict = new Dictionary<Type, IPacketHandler>();
                    _handlerBySpaces[handlerSpace] = dict;
                }

                dict[packetType] = handler;
            }
        }
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
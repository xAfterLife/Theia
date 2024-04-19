using System.Reflection;
using Communication.Packets.Attributes;

namespace Communication.Handlers;

public static class PacketProcessorFactory
{
    private static Dictionary<string, Dictionary<string, IPacketHandler>>? _handlerBySpaces;

    public static Dictionary<string, Dictionary<string, IPacketHandler>>? HandlerBySpaces
    {
        get
        {
            if ( _handlerBySpaces == null )
                LoadHandlers();

            return _handlerBySpaces;
        }
    }

    public static PacketProcessor CreateProcessor(string handlerSpace)
    {
        if ( HandlerBySpaces == null )
            return new PacketProcessor(handlerSpace, new Dictionary<string, IPacketHandler>());

        if ( !HandlerBySpaces!.TryGetValue(handlerSpace, out var packetHandlers) )
            packetHandlers ??= new Dictionary<string, IPacketHandler>();

        return new PacketProcessor(handlerSpace, packetHandlers);
    }

    public static void LoadHandlers()
    {
        _handlerBySpaces = new Dictionary<string, Dictionary<string, IPacketHandler>>();

        var types = Assembly.GetExecutingAssembly().GetTypes();

        foreach ( var type in types.Where(x => typeof(IPacketHandler).IsAssignableFrom(x) && x.GetCustomAttribute<PacketDefinitionAttribute>() != null) )
        {
            var attribute = type.GetCustomAttribute<PacketDefinitionAttribute>();
            if ( attribute == null )
                continue;

            if ( Activator.CreateInstance(type) is not IPacketHandler handler )
                continue;

            if ( !_handlerBySpaces.TryGetValue(attribute.HandlerSpace, out var dict) )
            {
                dict = new Dictionary<string, IPacketHandler>();
                _handlerBySpaces[attribute.HandlerSpace] = dict;
            }

            dict[attribute.Header] = handler;
        }
    }
}
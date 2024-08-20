using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Communication.Handlers;
using Communication.Packets;
using Communication.Packets.Attributes;
using Communication.Serialization;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using ExampleLib.Packets.MsgPacket;

namespace Benchmarking;

internal static class Program
{
    private static void Main()
    {
        BenchmarkRunner.Run<Benchmarks>();
    }
}

[MemoryDiagnoser]
public class Benchmarks
{
    [Benchmark]
    public IPacket? DecodeReflections()
    {
        var random = new Random(42069);
        IPacket? retPack = null;
        for ( var i = 0; i < 100_000; i++ )
        {
            var packet = Reflections.DeserializePacket(new Packet("msg", $"{random.Next()}^{random.Next()}{random.Next()}{random.Next()}{random.Next()}"));
            var msgPacket = (MsgPacket)packet;
            retPack = msgPacket;
        }
        return retPack;
    }

    [Benchmark(Baseline = true)]
    public IPacket? DecodeExpressions()
    {
        var random = new Random(42069);
        IPacket? retPack = null;
        for ( var i = 0; i < 100_000; i++ )
        {
            var packet = Expressions.DeserializePacket(new Packet("msg", $"{random.Next()}^{random.Next()}{random.Next()}{random.Next()}{random.Next()}"));
            var msgPacket = (MsgPacket)packet;
            retPack = msgPacket;
        }
        return retPack;
    }

    [Benchmark]
    public IPacket? DecodeCurrent()
    {
        var random = new Random(42069);
        IPacket? retPack = null;
        for ( var i = 0; i < 100_000; i++ )
        {
            var packet = PacketDeserializer.DeserializePacket(new Packet("msg", $"{random.Next()}^{random.Next()}{random.Next()}{random.Next()}{random.Next()}"));
            var msgPacket = (MsgPacket)packet;
            retPack = msgPacket;
        }
        return retPack;
    }

    [Benchmark]
    public IPacket? DecodeDynamic()
    {
        var random = new Random(42069);
        IPacket? retPack = null;
        for ( var i = 0; i < 100_000; i++ )
        {
            var packet = DynamicDeserializer.DeserializePacket(new Packet("msg", $"{random.Next()}^{random.Next()}{random.Next()}{random.Next()}{random.Next()}"));
            var msgPacket = (MsgPacket)packet;
            retPack = msgPacket;
        }
        return retPack;
    }
}







internal static class DynamicDeserializer
{
    private static readonly ConcurrentDictionary<string, Type> PacketTypes = new();
    private static readonly ConcurrentDictionary<Type, Func<Packet, IPacket?>> Deserializers = new();

    static DynamicDeserializer()
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

            PacketTypes.TryAdd(attribute.Header, type);
            GenerateDeserializer(type);
        }
    }

    private static bool IsPacket(Type type)
    {
        return typeof(IPacket).IsAssignableFrom(type) && type.GetCustomAttribute<PacketDefinitionAttribute>() != null;
    }

    private static void GenerateDeserializer(Type type)
    {
        var packetParameter = Expression.Parameter(typeof(Packet), "packet");
        var contentParameter = Expression.Variable(typeof(string[]), "content");
        var instanceVariable = Expression.Variable(type, "instance");

        var contentSplit = Expression.Assign(contentParameter, Expression.Call(Expression.Property(packetParameter, "Content"), typeof(string).GetMethod("Split", new[] { typeof(char[]) }), Expression.Constant(new[] { PacketConfiguration.ContentDelimiter })));
        var instanceCreation = Expression.Assign(instanceVariable, Expression.New(type));

        var block = new List<Expression> { contentSplit, instanceCreation };

        foreach ( var property in type.GetProperties() )
        {
            var indexAttribute = property.GetCustomAttribute<PacketIndexAttribute>();
            if ( indexAttribute == null )
                continue;

            var index = indexAttribute.Index;
            var value = Expression.ArrayIndex(contentParameter, Expression.Constant(index));
            var convertedValue = Expression.Convert(Expression.Call(typeof(Convert), "ChangeType", null, value, Expression.Constant(property.PropertyType)), property.PropertyType);
            var propertySetter = Expression.Assign(Expression.Property(instanceVariable, property), convertedValue);

            block.Add(propertySetter);
        }

        block.Add(instanceVariable);

        var lambda = Expression.Lambda<Func<Packet, IPacket?>>(Expression.Block(new[] { contentParameter, instanceVariable }, block), packetParameter);
        Deserializers.TryAdd(type, lambda.Compile());
    }

    public static IPacket? DeserializePacket(Packet packet)
    {
        if ( !PacketTypes.TryGetValue(packet.Header, out var type) )
            return null;

        if ( !Deserializers.TryGetValue(type, out var deserializer) )
            return null;

        return deserializer(packet);
    }
}
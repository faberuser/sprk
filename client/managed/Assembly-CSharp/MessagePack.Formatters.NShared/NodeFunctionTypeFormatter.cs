using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class NodeFunctionTypeFormatter : IMessagePackFormatter<NodeFunctionType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, NodeFunctionType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public NodeFunctionType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (NodeFunctionType)reader.ReadInt32();
	}
}

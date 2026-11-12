using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class OperationTypeFormatter : IMessagePackFormatter<OperationType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, OperationType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public OperationType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (OperationType)reader.ReadInt32();
	}
}

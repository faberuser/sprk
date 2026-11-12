using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class OperationTargetTypeFormatter : IMessagePackFormatter<OperationTargetType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, OperationTargetType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public OperationTargetType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (OperationTargetType)reader.ReadInt32();
	}
}

using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class OperationConditionTypeFormatter : IMessagePackFormatter<OperationConditionType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, OperationConditionType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public OperationConditionType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (OperationConditionType)reader.ReadInt32();
	}
}

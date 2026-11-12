using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class OperationGroupConditionTypeFormatter : IMessagePackFormatter<OperationGroupConditionType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, OperationGroupConditionType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public OperationGroupConditionType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (OperationGroupConditionType)reader.ReadInt32();
	}
}

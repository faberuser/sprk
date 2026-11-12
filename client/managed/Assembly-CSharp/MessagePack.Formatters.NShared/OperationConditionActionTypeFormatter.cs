using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class OperationConditionActionTypeFormatter : IMessagePackFormatter<OperationConditionActionType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, OperationConditionActionType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public OperationConditionActionType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (OperationConditionActionType)reader.ReadInt32();
	}
}

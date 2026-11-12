using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class StateConditionTypeFormatter : IMessagePackFormatter<StateConditionType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, StateConditionType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public StateConditionType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (StateConditionType)reader.ReadInt32();
	}
}

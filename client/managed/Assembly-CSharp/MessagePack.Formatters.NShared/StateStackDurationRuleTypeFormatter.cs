using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class StateStackDurationRuleTypeFormatter : IMessagePackFormatter<StateStackDurationRuleType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, StateStackDurationRuleType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public StateStackDurationRuleType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (StateStackDurationRuleType)reader.ReadInt32();
	}
}

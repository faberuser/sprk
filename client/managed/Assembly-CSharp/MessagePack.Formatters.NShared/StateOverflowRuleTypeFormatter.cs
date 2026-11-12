using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class StateOverflowRuleTypeFormatter : IMessagePackFormatter<StateOverflowRuleType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, StateOverflowRuleType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public StateOverflowRuleType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (StateOverflowRuleType)reader.ReadInt32();
	}
}

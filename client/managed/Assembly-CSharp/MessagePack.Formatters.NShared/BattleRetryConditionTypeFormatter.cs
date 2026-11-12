using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class BattleRetryConditionTypeFormatter : IMessagePackFormatter<BattleRetryConditionType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, BattleRetryConditionType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public BattleRetryConditionType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (BattleRetryConditionType)reader.ReadInt32();
	}
}

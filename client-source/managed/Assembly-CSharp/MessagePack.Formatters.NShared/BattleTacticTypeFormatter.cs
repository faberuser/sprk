using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class BattleTacticTypeFormatter : IMessagePackFormatter<BattleTacticType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, BattleTacticType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public BattleTacticType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (BattleTacticType)reader.ReadInt32();
	}
}

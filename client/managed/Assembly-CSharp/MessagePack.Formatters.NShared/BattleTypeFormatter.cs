using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class BattleTypeFormatter : IMessagePackFormatter<BattleType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, BattleType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public BattleType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (BattleType)reader.ReadInt32();
	}
}

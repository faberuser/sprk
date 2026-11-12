using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class StatTypeFormatter : IMessagePackFormatter<StatType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, StatType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public StatType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (StatType)reader.ReadInt32();
	}
}

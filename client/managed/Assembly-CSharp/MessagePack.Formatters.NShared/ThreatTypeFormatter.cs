using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ThreatTypeFormatter : IMessagePackFormatter<ThreatType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ThreatType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public ThreatType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (ThreatType)reader.ReadInt32();
	}
}

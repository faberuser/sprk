using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class RaidTypeFormatter : IMessagePackFormatter<RaidType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, RaidType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public RaidType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (RaidType)reader.ReadInt32();
	}
}

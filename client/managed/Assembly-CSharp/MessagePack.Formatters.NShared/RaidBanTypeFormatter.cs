using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class RaidBanTypeFormatter : IMessagePackFormatter<RaidBanType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, RaidBanType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public RaidBanType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (RaidBanType)reader.ReadInt32();
	}
}

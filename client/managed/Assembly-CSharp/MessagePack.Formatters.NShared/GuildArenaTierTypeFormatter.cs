using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class GuildArenaTierTypeFormatter : IMessagePackFormatter<GuildArenaTierType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, GuildArenaTierType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public GuildArenaTierType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (GuildArenaTierType)reader.ReadInt32();
	}
}

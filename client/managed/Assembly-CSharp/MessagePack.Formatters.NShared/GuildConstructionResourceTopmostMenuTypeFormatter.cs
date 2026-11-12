using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class GuildConstructionResourceTopmostMenuTypeFormatter : IMessagePackFormatter<GuildConstructionResourceTopmostMenuType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, GuildConstructionResourceTopmostMenuType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public GuildConstructionResourceTopmostMenuType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (GuildConstructionResourceTopmostMenuType)reader.ReadInt32();
	}
}

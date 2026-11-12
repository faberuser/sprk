using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class GuildSuppressServerRewardTypeFormatter : IMessagePackFormatter<GuildSuppressServerRewardType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, GuildSuppressServerRewardType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public GuildSuppressServerRewardType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (GuildSuppressServerRewardType)reader.ReadInt32();
	}
}

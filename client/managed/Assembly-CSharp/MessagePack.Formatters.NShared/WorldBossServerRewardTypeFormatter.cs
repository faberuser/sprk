using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class WorldBossServerRewardTypeFormatter : IMessagePackFormatter<WorldBossServerRewardType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, WorldBossServerRewardType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public WorldBossServerRewardType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (WorldBossServerRewardType)reader.ReadInt32();
	}
}

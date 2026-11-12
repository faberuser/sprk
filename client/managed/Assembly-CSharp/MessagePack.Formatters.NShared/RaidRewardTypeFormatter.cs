using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class RaidRewardTypeFormatter : IMessagePackFormatter<RaidRewardType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, RaidRewardType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public RaidRewardType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (RaidRewardType)reader.ReadInt32();
	}
}

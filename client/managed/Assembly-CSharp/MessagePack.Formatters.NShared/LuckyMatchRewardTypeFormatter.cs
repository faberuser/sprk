using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class LuckyMatchRewardTypeFormatter : IMessagePackFormatter<LuckyMatchRewardType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, LuckyMatchRewardType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public LuckyMatchRewardType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (LuckyMatchRewardType)reader.ReadInt32();
	}
}

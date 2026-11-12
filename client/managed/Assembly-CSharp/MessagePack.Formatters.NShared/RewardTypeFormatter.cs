using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class RewardTypeFormatter : IMessagePackFormatter<RewardType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, RewardType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public RewardType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (RewardType)reader.ReadInt32();
	}
}

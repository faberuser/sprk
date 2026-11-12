using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class RewardItemIconTypeFormatter : IMessagePackFormatter<RewardItemIconType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, RewardItemIconType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public RewardItemIconType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (RewardItemIconType)reader.ReadInt32();
	}
}

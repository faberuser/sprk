using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class MatchRewardTypeFormatter : IMessagePackFormatter<MatchRewardType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, MatchRewardType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public MatchRewardType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (MatchRewardType)reader.ReadInt32();
	}
}

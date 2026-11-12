using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class MatchBanPickStateFormatter : IMessagePackFormatter<MatchBanPickState>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, MatchBanPickState value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public MatchBanPickState Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (MatchBanPickState)reader.ReadInt32();
	}
}

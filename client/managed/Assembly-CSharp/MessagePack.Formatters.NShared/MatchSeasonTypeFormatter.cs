using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class MatchSeasonTypeFormatter : IMessagePackFormatter<MatchSeasonType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, MatchSeasonType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public MatchSeasonType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (MatchSeasonType)reader.ReadInt32();
	}
}

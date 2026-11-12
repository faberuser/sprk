using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class MatchTierTypeFormatter : IMessagePackFormatter<MatchTierType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, MatchTierType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public MatchTierType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (MatchTierType)reader.ReadInt32();
	}
}

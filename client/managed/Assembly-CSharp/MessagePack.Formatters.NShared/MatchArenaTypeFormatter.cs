using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class MatchArenaTypeFormatter : IMessagePackFormatter<MatchArenaType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, MatchArenaType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public MatchArenaType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (MatchArenaType)reader.ReadInt32();
	}
}

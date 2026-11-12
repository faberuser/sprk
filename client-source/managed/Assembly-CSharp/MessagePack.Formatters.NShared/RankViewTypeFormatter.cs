using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class RankViewTypeFormatter : IMessagePackFormatter<RankViewType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, RankViewType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public RankViewType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (RankViewType)reader.ReadInt32();
	}
}

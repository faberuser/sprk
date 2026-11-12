using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class RankPanelTypeFormatter : IMessagePackFormatter<RankPanelType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, RankPanelType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public RankPanelType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (RankPanelType)reader.ReadInt32();
	}
}

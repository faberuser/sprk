using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class MarketingTypeFormatter : IMessagePackFormatter<MarketingType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, MarketingType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public MarketingType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (MarketingType)reader.ReadInt32();
	}
}

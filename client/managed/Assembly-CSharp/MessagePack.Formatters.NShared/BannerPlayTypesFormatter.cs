using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class BannerPlayTypesFormatter : IMessagePackFormatter<BannerPlayTypes>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, BannerPlayTypes value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public BannerPlayTypes Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (BannerPlayTypes)reader.ReadInt32();
	}
}

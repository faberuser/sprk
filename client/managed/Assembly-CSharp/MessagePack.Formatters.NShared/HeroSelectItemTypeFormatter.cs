using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class HeroSelectItemTypeFormatter : IMessagePackFormatter<HeroSelectItemType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, HeroSelectItemType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public HeroSelectItemType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (HeroSelectItemType)reader.ReadInt32();
	}
}

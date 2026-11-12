using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EShopItemFilterGroupTypeFormatter : IMessagePackFormatter<EShopItemFilterGroupType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EShopItemFilterGroupType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public EShopItemFilterGroupType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (EShopItemFilterGroupType)reader.ReadInt32();
	}
}

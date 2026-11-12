using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EShopItemTypeFormatter : IMessagePackFormatter<EShopItemType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EShopItemType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public EShopItemType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (EShopItemType)reader.ReadInt32();
	}
}

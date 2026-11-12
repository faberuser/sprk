using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class NewPayShopProductCategoryTypeFormatter : IMessagePackFormatter<NewPayShopProductCategoryType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, NewPayShopProductCategoryType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public NewPayShopProductCategoryType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (NewPayShopProductCategoryType)reader.ReadInt32();
	}
}

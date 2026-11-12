using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class NewPayShopCategoryTypeFormatter : IMessagePackFormatter<NewPayShopCategoryType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, NewPayShopCategoryType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public NewPayShopCategoryType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (NewPayShopCategoryType)reader.ReadInt32();
	}
}

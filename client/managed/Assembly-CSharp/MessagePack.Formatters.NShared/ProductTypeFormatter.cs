using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ProductTypeFormatter : IMessagePackFormatter<ProductType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ProductType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public ProductType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (ProductType)reader.ReadInt32();
	}
}

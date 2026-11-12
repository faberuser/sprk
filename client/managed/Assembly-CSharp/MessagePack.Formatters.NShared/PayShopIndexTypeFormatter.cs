using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class PayShopIndexTypeFormatter : IMessagePackFormatter<PayShopIndexType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, PayShopIndexType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public PayShopIndexType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (PayShopIndexType)reader.ReadInt32();
	}
}

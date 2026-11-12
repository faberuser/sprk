using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EShopSortTypeFormatter : IMessagePackFormatter<EShopSortType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EShopSortType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public EShopSortType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (EShopSortType)reader.ReadInt32();
	}
}

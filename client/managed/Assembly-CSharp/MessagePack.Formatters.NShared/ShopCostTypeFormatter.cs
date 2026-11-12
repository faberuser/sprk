using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ShopCostTypeFormatter : IMessagePackFormatter<ShopCostType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ShopCostType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public ShopCostType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (ShopCostType)reader.ReadInt32();
	}
}

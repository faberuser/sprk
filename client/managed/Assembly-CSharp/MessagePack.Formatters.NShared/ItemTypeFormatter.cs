using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ItemTypeFormatter : IMessagePackFormatter<ItemType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ItemType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public ItemType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (ItemType)reader.ReadInt32();
	}
}

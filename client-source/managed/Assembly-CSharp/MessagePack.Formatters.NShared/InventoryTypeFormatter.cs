using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class InventoryTypeFormatter : IMessagePackFormatter<InventoryType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, InventoryType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public InventoryType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (InventoryType)reader.ReadInt32();
	}
}

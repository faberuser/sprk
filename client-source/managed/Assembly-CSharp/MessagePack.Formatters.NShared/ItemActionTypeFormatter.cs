using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ItemActionTypeFormatter : IMessagePackFormatter<ItemActionType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ItemActionType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public ItemActionType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (ItemActionType)reader.ReadInt32();
	}
}

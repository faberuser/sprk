using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ItemData_GetEffectTypeFormatter : IMessagePackFormatter<ItemData.GetEffectType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ItemData.GetEffectType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public ItemData.GetEffectType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (ItemData.GetEffectType)reader.ReadInt32();
	}
}

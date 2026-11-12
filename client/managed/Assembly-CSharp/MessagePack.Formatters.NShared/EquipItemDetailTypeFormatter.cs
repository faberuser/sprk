using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EquipItemDetailTypeFormatter : IMessagePackFormatter<EquipItemDetailType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EquipItemDetailType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public EquipItemDetailType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (EquipItemDetailType)reader.ReadInt32();
	}
}

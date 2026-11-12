using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EquipItemWorkPlaceFormatter : IMessagePackFormatter<EquipItemWorkPlace>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EquipItemWorkPlace value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public EquipItemWorkPlace Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (EquipItemWorkPlace)reader.ReadInt32();
	}
}

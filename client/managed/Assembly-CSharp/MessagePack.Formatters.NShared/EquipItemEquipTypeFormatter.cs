using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EquipItemEquipTypeFormatter : IMessagePackFormatter<EquipItemEquipType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EquipItemEquipType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public EquipItemEquipType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (EquipItemEquipType)reader.ReadInt32();
	}
}

using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EquipItemSetGroupTypeFormatter : IMessagePackFormatter<EquipItemSetGroupType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EquipItemSetGroupType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public EquipItemSetGroupType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (EquipItemSetGroupType)reader.ReadInt32();
	}
}

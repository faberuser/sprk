using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EquipPartTypeFlagFormatter : IMessagePackFormatter<EquipPartTypeFlag>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EquipPartTypeFlag value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public EquipPartTypeFlag Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (EquipPartTypeFlag)reader.ReadInt32();
	}
}

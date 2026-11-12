using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EquipPartTypeFormatter : IMessagePackFormatter<EquipPartType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EquipPartType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public EquipPartType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (EquipPartType)reader.ReadInt32();
	}
}

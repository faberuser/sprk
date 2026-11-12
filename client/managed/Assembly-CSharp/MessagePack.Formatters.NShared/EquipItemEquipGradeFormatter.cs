using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EquipItemEquipGradeFormatter : IMessagePackFormatter<EquipItemEquipGrade>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EquipItemEquipGrade value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public EquipItemEquipGrade Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (EquipItemEquipGrade)reader.ReadInt32();
	}
}

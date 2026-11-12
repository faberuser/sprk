using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EEquipConditionFormatter : IMessagePackFormatter<EEquipCondition>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EEquipCondition value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public EEquipCondition Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (EEquipCondition)reader.ReadInt32();
	}
}

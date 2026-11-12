using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EquipGachaSubTypeFormatter : IMessagePackFormatter<EquipGachaSubType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EquipGachaSubType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public EquipGachaSubType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (EquipGachaSubType)reader.ReadInt32();
	}
}

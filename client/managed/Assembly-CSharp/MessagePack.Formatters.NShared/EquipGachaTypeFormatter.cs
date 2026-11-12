using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EquipGachaTypeFormatter : IMessagePackFormatter<EquipGachaType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EquipGachaType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public EquipGachaType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (EquipGachaType)reader.ReadInt32();
	}
}

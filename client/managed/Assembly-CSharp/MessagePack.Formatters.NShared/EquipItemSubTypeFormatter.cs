using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EquipItemSubTypeFormatter : IMessagePackFormatter<EquipItemSubType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EquipItemSubType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public EquipItemSubType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (EquipItemSubType)reader.ReadInt32();
	}
}

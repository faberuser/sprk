using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EquipTransitionTypeFormatter : IMessagePackFormatter<EquipTransitionType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EquipTransitionType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public EquipTransitionType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (EquipTransitionType)reader.ReadInt32();
	}
}

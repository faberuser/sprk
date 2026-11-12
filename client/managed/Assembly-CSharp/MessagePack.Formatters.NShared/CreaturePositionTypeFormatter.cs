using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class CreaturePositionTypeFormatter : IMessagePackFormatter<CreaturePositionType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, CreaturePositionType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public CreaturePositionType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (CreaturePositionType)reader.ReadInt32();
	}
}

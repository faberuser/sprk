using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class CreatureOpenStatusTypeFormatter : IMessagePackFormatter<CreatureOpenStatusType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, CreatureOpenStatusType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public CreatureOpenStatusType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (CreatureOpenStatusType)reader.ReadInt32();
	}
}

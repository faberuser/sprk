using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class CreatureTagTypeFormatter : IMessagePackFormatter<CreatureTagType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, CreatureTagType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public CreatureTagType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (CreatureTagType)reader.ReadInt32();
	}
}

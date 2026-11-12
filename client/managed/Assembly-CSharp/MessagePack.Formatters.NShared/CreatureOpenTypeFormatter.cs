using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class CreatureOpenTypeFormatter : IMessagePackFormatter<CreatureOpenType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, CreatureOpenType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public CreatureOpenType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (CreatureOpenType)reader.ReadInt32();
	}
}

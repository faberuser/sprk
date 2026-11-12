using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class UnderPrisonTypeFormatter : IMessagePackFormatter<UnderPrisonType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, UnderPrisonType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public UnderPrisonType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (UnderPrisonType)reader.ReadInt32();
	}
}

using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EOrdealEventTypeFormatter : IMessagePackFormatter<EOrdealEventType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EOrdealEventType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public EOrdealEventType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (EOrdealEventType)reader.ReadInt32();
	}
}

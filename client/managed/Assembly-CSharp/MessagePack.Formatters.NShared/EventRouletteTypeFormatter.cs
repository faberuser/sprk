using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EventRouletteTypeFormatter : IMessagePackFormatter<EventRouletteType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EventRouletteType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public EventRouletteType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (EventRouletteType)reader.ReadInt32();
	}
}

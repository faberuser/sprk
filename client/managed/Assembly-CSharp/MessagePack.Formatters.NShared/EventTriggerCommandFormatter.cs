using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EventTriggerCommandFormatter : IMessagePackFormatter<EventTriggerCommand>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EventTriggerCommand value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public EventTriggerCommand Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (EventTriggerCommand)reader.ReadInt32();
	}
}

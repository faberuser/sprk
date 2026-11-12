using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EventTriggerCategoryFormatter : IMessagePackFormatter<EventTriggerCategory>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EventTriggerCategory value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public EventTriggerCategory Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (EventTriggerCategory)reader.ReadInt32();
	}
}

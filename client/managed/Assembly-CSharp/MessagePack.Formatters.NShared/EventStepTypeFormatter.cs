using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EventStepTypeFormatter : IMessagePackFormatter<EventStepType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EventStepType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public EventStepType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (EventStepType)reader.ReadInt32();
	}
}

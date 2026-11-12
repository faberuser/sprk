using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EventDungeonPropTypeFormatter : IMessagePackFormatter<EventDungeonPropType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EventDungeonPropType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public EventDungeonPropType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (EventDungeonPropType)reader.ReadInt32();
	}
}

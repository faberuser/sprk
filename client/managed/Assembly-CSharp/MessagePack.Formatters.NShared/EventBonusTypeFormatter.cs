using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EventBonusTypeFormatter : IMessagePackFormatter<EventBonusType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EventBonusType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public EventBonusType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (EventBonusType)reader.ReadInt32();
	}
}

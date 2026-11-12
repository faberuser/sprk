using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class CalendarCutSceneTypeFormatter : IMessagePackFormatter<CalendarCutSceneType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, CalendarCutSceneType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public CalendarCutSceneType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (CalendarCutSceneType)reader.ReadInt32();
	}
}

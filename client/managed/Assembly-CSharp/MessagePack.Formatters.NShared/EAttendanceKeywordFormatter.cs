using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EAttendanceKeywordFormatter : IMessagePackFormatter<EAttendanceKeyword>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EAttendanceKeyword value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public EAttendanceKeyword Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (EAttendanceKeyword)reader.ReadInt32();
	}
}

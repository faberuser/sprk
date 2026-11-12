using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ChapterTypeFormatter : IMessagePackFormatter<ChapterType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ChapterType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public ChapterType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (ChapterType)reader.ReadInt32();
	}
}

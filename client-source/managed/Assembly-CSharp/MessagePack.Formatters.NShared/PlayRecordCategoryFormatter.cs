using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class PlayRecordCategoryFormatter : IMessagePackFormatter<PlayRecordCategory>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, PlayRecordCategory value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public PlayRecordCategory Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (PlayRecordCategory)reader.ReadInt32();
	}
}

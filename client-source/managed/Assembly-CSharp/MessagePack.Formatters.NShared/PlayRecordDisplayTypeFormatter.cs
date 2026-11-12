using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class PlayRecordDisplayTypeFormatter : IMessagePackFormatter<PlayRecordDisplayType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, PlayRecordDisplayType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public PlayRecordDisplayType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (PlayRecordDisplayType)reader.ReadInt32();
	}
}

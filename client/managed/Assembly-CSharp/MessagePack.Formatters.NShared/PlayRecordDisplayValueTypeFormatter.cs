using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class PlayRecordDisplayValueTypeFormatter : IMessagePackFormatter<PlayRecordDisplayValueType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, PlayRecordDisplayValueType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public PlayRecordDisplayValueType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (PlayRecordDisplayValueType)reader.ReadInt32();
	}
}

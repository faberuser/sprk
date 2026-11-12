using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class PlayRecordActionCalculateTypeFormatter : IMessagePackFormatter<PlayRecordActionCalculateType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, PlayRecordActionCalculateType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public PlayRecordActionCalculateType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (PlayRecordActionCalculateType)reader.ReadInt32();
	}
}

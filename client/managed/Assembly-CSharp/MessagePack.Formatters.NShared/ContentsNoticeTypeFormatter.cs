using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ContentsNoticeTypeFormatter : IMessagePackFormatter<ContentsNoticeType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ContentsNoticeType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public ContentsNoticeType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (ContentsNoticeType)reader.ReadInt32();
	}
}

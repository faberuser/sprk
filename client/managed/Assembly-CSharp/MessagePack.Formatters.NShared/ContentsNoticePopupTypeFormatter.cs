using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ContentsNoticePopupTypeFormatter : IMessagePackFormatter<ContentsNoticePopupType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ContentsNoticePopupType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public ContentsNoticePopupType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (ContentsNoticePopupType)reader.ReadInt32();
	}
}

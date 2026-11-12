using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ContentsStatusTypeFormatter : IMessagePackFormatter<ContentsStatusType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ContentsStatusType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public ContentsStatusType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (ContentsStatusType)reader.ReadInt32();
	}
}

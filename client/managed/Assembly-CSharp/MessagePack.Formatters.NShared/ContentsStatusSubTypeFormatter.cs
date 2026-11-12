using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ContentsStatusSubTypeFormatter : IMessagePackFormatter<ContentsStatusSubType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ContentsStatusSubType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public ContentsStatusSubType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (ContentsStatusSubType)reader.ReadInt32();
	}
}

using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ContentsTypeFormatter : IMessagePackFormatter<ContentsType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ContentsType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public ContentsType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (ContentsType)reader.ReadInt32();
	}
}

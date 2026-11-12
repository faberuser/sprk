using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class RecordsOfHonorContentTypeFormatter : IMessagePackFormatter<RecordsOfHonorContentType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, RecordsOfHonorContentType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public RecordsOfHonorContentType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (RecordsOfHonorContentType)reader.ReadInt32();
	}
}

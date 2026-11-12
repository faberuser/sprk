using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EContentTypeFormatter : IMessagePackFormatter<EContentType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EContentType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public EContentType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (EContentType)reader.ReadInt32();
	}
}

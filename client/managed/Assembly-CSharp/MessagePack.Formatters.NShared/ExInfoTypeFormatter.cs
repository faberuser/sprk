using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ExInfoTypeFormatter : IMessagePackFormatter<ExInfoType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ExInfoType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public ExInfoType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (ExInfoType)reader.ReadInt32();
	}
}

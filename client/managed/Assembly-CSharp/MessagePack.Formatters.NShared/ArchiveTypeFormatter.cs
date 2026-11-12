using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ArchiveTypeFormatter : IMessagePackFormatter<ArchiveType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ArchiveType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public ArchiveType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (ArchiveType)reader.ReadInt32();
	}
}

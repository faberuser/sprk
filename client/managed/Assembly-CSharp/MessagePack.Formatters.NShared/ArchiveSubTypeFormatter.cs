using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ArchiveSubTypeFormatter : IMessagePackFormatter<ArchiveSubType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ArchiveSubType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public ArchiveSubType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (ArchiveSubType)reader.ReadInt32();
	}
}

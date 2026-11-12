using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ArchiveSubType2Formatter : IMessagePackFormatter<ArchiveSubType2>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ArchiveSubType2 value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public ArchiveSubType2 Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (ArchiveSubType2)reader.ReadInt32();
	}
}

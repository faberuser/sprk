using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ArchiveMainTypeFormatter : IMessagePackFormatter<ArchiveMainType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ArchiveMainType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public ArchiveMainType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (ArchiveMainType)reader.ReadInt32();
	}
}

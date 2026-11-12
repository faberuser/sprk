using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ContentsStateIconFormatter : IMessagePackFormatter<ContentsStateIcon>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ContentsStateIcon value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public ContentsStateIcon Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (ContentsStateIcon)reader.ReadInt32();
	}
}

using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ERightMenuButtonFormatter : IMessagePackFormatter<ERightMenuButton>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ERightMenuButton value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public ERightMenuButton Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (ERightMenuButton)reader.ReadInt32();
	}
}

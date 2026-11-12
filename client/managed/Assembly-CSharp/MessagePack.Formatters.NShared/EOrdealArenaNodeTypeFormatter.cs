using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EOrdealArenaNodeTypeFormatter : IMessagePackFormatter<EOrdealArenaNodeType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EOrdealArenaNodeType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public EOrdealArenaNodeType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (EOrdealArenaNodeType)reader.ReadInt32();
	}
}

using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class FlaskTypeFormatter : IMessagePackFormatter<FlaskType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, FlaskType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public FlaskType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (FlaskType)reader.ReadInt32();
	}
}

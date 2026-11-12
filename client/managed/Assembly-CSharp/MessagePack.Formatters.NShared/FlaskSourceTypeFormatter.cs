using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class FlaskSourceTypeFormatter : IMessagePackFormatter<FlaskSourceType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, FlaskSourceType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public FlaskSourceType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (FlaskSourceType)reader.ReadInt32();
	}
}

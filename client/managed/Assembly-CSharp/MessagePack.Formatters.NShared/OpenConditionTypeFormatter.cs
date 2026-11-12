using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class OpenConditionTypeFormatter : IMessagePackFormatter<OpenConditionType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, OpenConditionType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public OpenConditionType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (OpenConditionType)reader.ReadInt32();
	}
}

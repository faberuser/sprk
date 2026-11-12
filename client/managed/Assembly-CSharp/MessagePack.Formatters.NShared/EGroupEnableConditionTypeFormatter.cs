using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EGroupEnableConditionTypeFormatter : IMessagePackFormatter<EGroupEnableConditionType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EGroupEnableConditionType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public EGroupEnableConditionType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (EGroupEnableConditionType)reader.ReadInt32();
	}
}

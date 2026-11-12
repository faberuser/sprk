using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EDelayTypeFormatter : IMessagePackFormatter<EDelayType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EDelayType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public EDelayType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (EDelayType)reader.ReadInt32();
	}
}

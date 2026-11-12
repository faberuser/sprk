using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class HighGachaCategoryTypeFormatter : IMessagePackFormatter<HighGachaCategoryType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, HighGachaCategoryType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public HighGachaCategoryType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (HighGachaCategoryType)reader.ReadInt32();
	}
}

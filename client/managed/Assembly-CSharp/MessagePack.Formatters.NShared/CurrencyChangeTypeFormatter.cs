using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class CurrencyChangeTypeFormatter : IMessagePackFormatter<CurrencyChangeType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, CurrencyChangeType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public CurrencyChangeType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (CurrencyChangeType)reader.ReadInt32();
	}
}

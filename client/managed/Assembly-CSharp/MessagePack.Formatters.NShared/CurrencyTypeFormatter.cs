using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class CurrencyTypeFormatter : IMessagePackFormatter<CurrencyType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, CurrencyType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public CurrencyType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (CurrencyType)reader.ReadInt32();
	}
}

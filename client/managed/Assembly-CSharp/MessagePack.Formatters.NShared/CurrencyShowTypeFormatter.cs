using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class CurrencyShowTypeFormatter : IMessagePackFormatter<CurrencyShowType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, CurrencyShowType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public CurrencyShowType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (CurrencyShowType)reader.ReadInt32();
	}
}

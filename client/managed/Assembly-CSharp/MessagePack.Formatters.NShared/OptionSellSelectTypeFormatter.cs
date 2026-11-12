using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class OptionSellSelectTypeFormatter : IMessagePackFormatter<OptionSellSelectType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, OptionSellSelectType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public OptionSellSelectType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (OptionSellSelectType)reader.ReadInt32();
	}
}

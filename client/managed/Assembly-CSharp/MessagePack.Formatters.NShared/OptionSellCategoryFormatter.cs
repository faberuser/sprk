using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class OptionSellCategoryFormatter : IMessagePackFormatter<OptionSellCategory>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, OptionSellCategory value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public OptionSellCategory Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (OptionSellCategory)reader.ReadInt32();
	}
}

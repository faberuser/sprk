using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class LuckyMatchTierTypeFormatter : IMessagePackFormatter<LuckyMatchTierType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, LuckyMatchTierType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public LuckyMatchTierType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (LuckyMatchTierType)reader.ReadInt32();
	}
}

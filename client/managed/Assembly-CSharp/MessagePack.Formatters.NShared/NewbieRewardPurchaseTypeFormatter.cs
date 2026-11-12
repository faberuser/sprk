using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class NewbieRewardPurchaseTypeFormatter : IMessagePackFormatter<NewbieRewardPurchaseType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, NewbieRewardPurchaseType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public NewbieRewardPurchaseType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (NewbieRewardPurchaseType)reader.ReadInt32();
	}
}

using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class NewbieRewardTypeFormatter : IMessagePackFormatter<NewbieRewardType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, NewbieRewardType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public NewbieRewardType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (NewbieRewardType)reader.ReadInt32();
	}
}

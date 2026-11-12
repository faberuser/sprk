using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class NewbieRewardProvideTypeFormatter : IMessagePackFormatter<NewbieRewardProvideType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, NewbieRewardProvideType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public NewbieRewardProvideType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (NewbieRewardProvideType)reader.ReadInt32();
	}
}

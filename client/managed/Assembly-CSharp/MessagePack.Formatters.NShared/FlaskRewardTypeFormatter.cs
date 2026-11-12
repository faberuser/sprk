using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class FlaskRewardTypeFormatter : IMessagePackFormatter<FlaskRewardType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, FlaskRewardType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public FlaskRewardType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (FlaskRewardType)reader.ReadInt32();
	}
}

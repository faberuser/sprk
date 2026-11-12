using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EMissionTopViewRewardTypeFormatter : IMessagePackFormatter<EMissionTopViewRewardType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EMissionTopViewRewardType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public EMissionTopViewRewardType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (EMissionTopViewRewardType)reader.ReadInt32();
	}
}

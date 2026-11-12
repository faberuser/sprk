using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class AchievementRewardTypeFormatter : IMessagePackFormatter<AchievementRewardType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, AchievementRewardType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public AchievementRewardType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (AchievementRewardType)reader.ReadInt32();
	}
}

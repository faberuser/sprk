using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class AchievementSubTypeFormatter : IMessagePackFormatter<AchievementSubType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, AchievementSubType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public AchievementSubType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (AchievementSubType)reader.ReadInt32();
	}
}

using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class AchievementCategoryTypeFormatter : IMessagePackFormatter<AchievementCategoryType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, AchievementCategoryType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public AchievementCategoryType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (AchievementCategoryType)reader.ReadInt32();
	}
}

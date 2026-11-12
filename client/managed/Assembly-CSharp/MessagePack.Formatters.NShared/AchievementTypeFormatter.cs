using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class AchievementTypeFormatter : IMessagePackFormatter<AchievementType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, AchievementType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public AchievementType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (AchievementType)reader.ReadInt32();
	}
}

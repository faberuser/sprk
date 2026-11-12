using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class AchievementReqSubTypeFormatter : IMessagePackFormatter<AchievementReqSubType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, AchievementReqSubType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public AchievementReqSubType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (AchievementReqSubType)reader.ReadInt32();
	}
}

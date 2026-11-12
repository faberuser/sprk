using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class AchievementReqTypeFormatter : IMessagePackFormatter<AchievementReqType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, AchievementReqType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public AchievementReqType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (AchievementReqType)reader.ReadInt32();
	}
}

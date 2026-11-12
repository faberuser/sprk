using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class QuestOpenConditionTypeFormatter : IMessagePackFormatter<QuestOpenConditionType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, QuestOpenConditionType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public QuestOpenConditionType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (QuestOpenConditionType)reader.ReadInt32();
	}
}

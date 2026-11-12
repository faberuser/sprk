using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class QuestActionTypeFormatter : IMessagePackFormatter<QuestActionType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, QuestActionType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public QuestActionType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (QuestActionType)reader.ReadInt32();
	}
}

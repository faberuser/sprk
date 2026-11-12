using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class MonsterGradeTypeFormatter : IMessagePackFormatter<MonsterGradeType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, MonsterGradeType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public MonsterGradeType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (MonsterGradeType)reader.ReadInt32();
	}
}

using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class SkillTargetTypeFormatter : IMessagePackFormatter<SkillTargetType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, SkillTargetType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public SkillTargetType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (SkillTargetType)reader.ReadInt32();
	}
}

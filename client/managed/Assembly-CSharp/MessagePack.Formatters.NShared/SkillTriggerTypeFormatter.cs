using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class SkillTriggerTypeFormatter : IMessagePackFormatter<SkillTriggerType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, SkillTriggerType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public SkillTriggerType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (SkillTriggerType)reader.ReadInt32();
	}
}

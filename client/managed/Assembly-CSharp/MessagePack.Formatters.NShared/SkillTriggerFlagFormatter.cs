using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class SkillTriggerFlagFormatter : IMessagePackFormatter<SkillTriggerFlag>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, SkillTriggerFlag value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public SkillTriggerFlag Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (SkillTriggerFlag)reader.ReadInt32();
	}
}

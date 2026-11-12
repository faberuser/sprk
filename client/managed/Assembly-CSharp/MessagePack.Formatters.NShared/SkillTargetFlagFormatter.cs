using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class SkillTargetFlagFormatter : IMessagePackFormatter<SkillTargetFlag>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, SkillTargetFlag value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public SkillTargetFlag Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (SkillTargetFlag)reader.ReadInt32();
	}
}

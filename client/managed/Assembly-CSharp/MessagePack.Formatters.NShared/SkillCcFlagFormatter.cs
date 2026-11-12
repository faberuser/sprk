using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class SkillCcFlagFormatter : IMessagePackFormatter<SkillCcFlag>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, SkillCcFlag value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public SkillCcFlag Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (SkillCcFlag)reader.ReadInt32();
	}
}

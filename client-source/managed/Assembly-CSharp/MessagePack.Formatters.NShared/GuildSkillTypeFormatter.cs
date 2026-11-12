using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class GuildSkillTypeFormatter : IMessagePackFormatter<GuildSkillType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, GuildSkillType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public GuildSkillType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (GuildSkillType)reader.ReadInt32();
	}
}

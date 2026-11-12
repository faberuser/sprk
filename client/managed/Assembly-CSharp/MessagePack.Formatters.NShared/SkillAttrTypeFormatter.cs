using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class SkillAttrTypeFormatter : IMessagePackFormatter<SkillAttrType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, SkillAttrType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public SkillAttrType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (SkillAttrType)reader.ReadInt32();
	}
}

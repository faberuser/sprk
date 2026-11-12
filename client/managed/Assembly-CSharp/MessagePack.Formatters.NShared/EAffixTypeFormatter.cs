using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EAffixTypeFormatter : IMessagePackFormatter<EAffixType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EAffixType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public EAffixType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (EAffixType)reader.ReadInt32();
	}
}

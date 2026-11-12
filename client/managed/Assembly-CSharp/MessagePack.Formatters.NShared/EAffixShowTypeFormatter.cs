using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EAffixShowTypeFormatter : IMessagePackFormatter<EAffixShowType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EAffixShowType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public EAffixShowType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (EAffixShowType)reader.ReadInt32();
	}
}

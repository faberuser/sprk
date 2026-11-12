using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EValanceFilterGroupTypeFormatter : IMessagePackFormatter<EValanceFilterGroupType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EValanceFilterGroupType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public EValanceFilterGroupType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (EValanceFilterGroupType)reader.ReadInt32();
	}
}

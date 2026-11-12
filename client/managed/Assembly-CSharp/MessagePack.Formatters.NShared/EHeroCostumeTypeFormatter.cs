using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EHeroCostumeTypeFormatter : IMessagePackFormatter<EHeroCostumeType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EHeroCostumeType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public EHeroCostumeType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (EHeroCostumeType)reader.ReadInt32();
	}
}

using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class CostumeAbilityTypeFormatter : IMessagePackFormatter<CostumeAbilityType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, CostumeAbilityType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public CostumeAbilityType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (CostumeAbilityType)reader.ReadInt32();
	}
}

using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class PetOptionConditionTypeFormatter : IMessagePackFormatter<PetOptionConditionType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, PetOptionConditionType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public PetOptionConditionType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (PetOptionConditionType)reader.ReadInt32();
	}
}

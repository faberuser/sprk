using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ECostumeBuffTypeFormatter : IMessagePackFormatter<ECostumeBuffType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ECostumeBuffType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public ECostumeBuffType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (ECostumeBuffType)reader.ReadInt32();
	}
}

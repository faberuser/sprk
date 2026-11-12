using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ECostumeSelectItemTypeFormatter : IMessagePackFormatter<ECostumeSelectItemType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ECostumeSelectItemType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public ECostumeSelectItemType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (ECostumeSelectItemType)reader.ReadInt32();
	}
}

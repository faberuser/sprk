using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EPetAdventureTypeFormatter : IMessagePackFormatter<EPetAdventureType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EPetAdventureType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public EPetAdventureType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (EPetAdventureType)reader.ReadInt32();
	}
}

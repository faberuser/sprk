using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class CreatureRaceTypeFormatter : IMessagePackFormatter<CreatureRaceType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, CreatureRaceType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public CreatureRaceType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (CreatureRaceType)reader.ReadInt32();
	}
}

using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class PartyDungeonTypeFormatter : IMessagePackFormatter<PartyDungeonType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, PartyDungeonType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public PartyDungeonType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (PartyDungeonType)reader.ReadInt32();
	}
}

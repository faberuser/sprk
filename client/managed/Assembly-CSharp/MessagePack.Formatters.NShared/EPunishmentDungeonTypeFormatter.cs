using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EPunishmentDungeonTypeFormatter : IMessagePackFormatter<EPunishmentDungeonType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EPunishmentDungeonType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public EPunishmentDungeonType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (EPunishmentDungeonType)reader.ReadInt32();
	}
}

using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EPunishmentPortraitLineTypeFormatter : IMessagePackFormatter<EPunishmentPortraitLineType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EPunishmentPortraitLineType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public EPunishmentPortraitLineType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (EPunishmentPortraitLineType)reader.ReadInt32();
	}
}

using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class PunishmentRaidRewardTypeFormatter : IMessagePackFormatter<PunishmentRaidRewardType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, PunishmentRaidRewardType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public PunishmentRaidRewardType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (PunishmentRaidRewardType)reader.ReadInt32();
	}
}

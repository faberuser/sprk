using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ESoulWeaponAbilityTicketTypeFormatter : IMessagePackFormatter<ESoulWeaponAbilityTicketType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ESoulWeaponAbilityTicketType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public ESoulWeaponAbilityTicketType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (ESoulWeaponAbilityTicketType)reader.ReadInt32();
	}
}

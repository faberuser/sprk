using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class SoulWeaponAbilityTicketDataFormatter : IMessagePackFormatter<SoulWeaponAbilityTicketData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, SoulWeaponAbilityTicketData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(5);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ItemCode, options);
		resolver.GetFormatterWithVerify<ESoulWeaponAbilityTicketType>().Serialize(ref writer, value.ItemType, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.FixedEquipItemIndex, options);
		writer.Write(value.SoulWeaponGrade);
		writer.Write(value.EtherReinforceLevel);
	}

	public SoulWeaponAbilityTicketData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		SoulWeaponAbilityTicketData soulWeaponAbilityTicketData = new SoulWeaponAbilityTicketData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				soulWeaponAbilityTicketData.ItemCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 1:
				soulWeaponAbilityTicketData.ItemType = resolver.GetFormatterWithVerify<ESoulWeaponAbilityTicketType>().Deserialize(ref reader, options);
				break;
			case 2:
				soulWeaponAbilityTicketData.FixedEquipItemIndex = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 3:
				soulWeaponAbilityTicketData.SoulWeaponGrade = reader.ReadInt32();
				break;
			case 4:
				soulWeaponAbilityTicketData.EtherReinforceLevel = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return soulWeaponAbilityTicketData;
	}
}

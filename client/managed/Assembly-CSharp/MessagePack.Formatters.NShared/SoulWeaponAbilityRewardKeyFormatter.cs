using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class SoulWeaponAbilityRewardKeyFormatter : IMessagePackFormatter<SoulWeaponAbilityRewardKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, SoulWeaponAbilityRewardKey value, MessagePackSerializerOptions options)
	{
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(4);
		resolver.GetFormatterWithVerify<ESoulWeaponAbilityTicketType>().Serialize(ref writer, value.TicketType, options);
		resolver.GetFormatterWithVerify<CreatureTagType>().Serialize(ref writer, value.TagType, options);
		writer.Write(value.Grade);
		writer.Write(value.ReinforceLevel);
	}

	public SoulWeaponAbilityRewardKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		ESoulWeaponAbilityTicketType ticketType = ESoulWeaponAbilityTicketType.None;
		CreatureTagType tagType = CreatureTagType.None;
		int grade = 0;
		int reinforceLevel = 0;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				ticketType = resolver.GetFormatterWithVerify<ESoulWeaponAbilityTicketType>().Deserialize(ref reader, options);
				break;
			case 1:
				tagType = resolver.GetFormatterWithVerify<CreatureTagType>().Deserialize(ref reader, options);
				break;
			case 2:
				grade = reader.ReadInt32();
				break;
			case 3:
				reinforceLevel = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		SoulWeaponAbilityRewardKey result = new SoulWeaponAbilityRewardKey(ticketType, tagType, grade, reinforceLevel);
		reader.Depth--;
		return result;
	}
}

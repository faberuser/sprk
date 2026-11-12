using System.Collections.Generic;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class SoulWeaponAbilityRewardDataFormatter : IMessagePackFormatter<SoulWeaponAbilityRewardData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, SoulWeaponAbilityRewardData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(6);
		resolver.GetFormatterWithVerify<ESoulWeaponAbilityTicketType>().Serialize(ref writer, value.Type, options);
		resolver.GetFormatterWithVerify<CreatureTagType>().Serialize(ref writer, value.TagType, options);
		writer.Write(value.Grade);
		writer.Write(value.ReinforceLevel);
		resolver.GetFormatterWithVerify<List<string>>().Serialize(ref writer, value.ItemCodeList, options);
		resolver.GetFormatterWithVerify<List<int>>().Serialize(ref writer, value.ItemCountList, options);
	}

	public SoulWeaponAbilityRewardData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		SoulWeaponAbilityRewardData soulWeaponAbilityRewardData = new SoulWeaponAbilityRewardData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				soulWeaponAbilityRewardData.Type = resolver.GetFormatterWithVerify<ESoulWeaponAbilityTicketType>().Deserialize(ref reader, options);
				break;
			case 1:
				soulWeaponAbilityRewardData.TagType = resolver.GetFormatterWithVerify<CreatureTagType>().Deserialize(ref reader, options);
				break;
			case 2:
				soulWeaponAbilityRewardData.Grade = reader.ReadInt32();
				break;
			case 3:
				soulWeaponAbilityRewardData.ReinforceLevel = reader.ReadInt32();
				break;
			case 4:
				soulWeaponAbilityRewardData.ItemCodeList = resolver.GetFormatterWithVerify<List<string>>().Deserialize(ref reader, options);
				break;
			case 5:
				soulWeaponAbilityRewardData.ItemCountList = resolver.GetFormatterWithVerify<List<int>>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return soulWeaponAbilityRewardData;
	}
}

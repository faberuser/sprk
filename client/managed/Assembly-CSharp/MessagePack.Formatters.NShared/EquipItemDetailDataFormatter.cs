using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EquipItemDetailDataFormatter : IMessagePackFormatter<EquipItemDetailData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EquipItemDetailData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(28);
		writer.Write(value.DetailIndex);
		resolver.GetFormatterWithVerify<EquipItemDetailType>().Serialize(ref writer, value.EquipItemDetailType, options);
		writer.Write(value.IsUnique);
		writer.Write(value.MaxLevel);
		writer.Write(value.MaxLevelByHero);
		writer.Write(value.ReqAwakeMaterialLevel);
		writer.Write(value.MaxStar);
		writer.Write(value.OptionCount);
		writer.Write(value.AllOptionOpen);
		writer.Write(value.RenewEnable);
		writer.Write(value.TotalOptionStepUpgradeCount);
		writer.Write(value.RenewableOptionCount);
		resolver.GetFormatterWithVerify<CurrencyType>().Serialize(ref writer, value.RenewCurrencyType, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.RenewPrice, options);
		writer.Write(value.RenewTicketItemIndex);
		writer.Write(value.EnableRenewSameOption);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ForgeAwakeSuccessNodeTree, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ForgeAwakeFailNodeTree, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ForgeRenewSuccessNodeTree, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ForgeEnchantSuccessNodeTree, options);
		writer.Write(value.RenewItemIndex1);
		writer.Write(value.RenewItemCount1);
		writer.Write(value.RenewItemIndex2);
		writer.Write(value.RenewItemCount2);
		writer.Write(value.RenewItemIndex3);
		writer.Write(value.RenewItemCount3);
		writer.Write(value.ExtraSkillRenewItemIndex);
		writer.Write(value.ExtraSkillRenewItemCount);
	}

	public EquipItemDetailData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		EquipItemDetailData equipItemDetailData = new EquipItemDetailData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				equipItemDetailData.DetailIndex = reader.ReadInt32();
				break;
			case 1:
				equipItemDetailData.EquipItemDetailType = resolver.GetFormatterWithVerify<EquipItemDetailType>().Deserialize(ref reader, options);
				break;
			case 2:
				equipItemDetailData.IsUnique = reader.ReadBoolean();
				break;
			case 3:
				equipItemDetailData.MaxLevel = reader.ReadInt32();
				break;
			case 4:
				equipItemDetailData.MaxLevelByHero = reader.ReadBoolean();
				break;
			case 5:
				equipItemDetailData.ReqAwakeMaterialLevel = reader.ReadInt32();
				break;
			case 6:
				equipItemDetailData.MaxStar = reader.ReadInt32();
				break;
			case 7:
				equipItemDetailData.OptionCount = reader.ReadInt32();
				break;
			case 8:
				equipItemDetailData.AllOptionOpen = reader.ReadBoolean();
				break;
			case 9:
				equipItemDetailData.RenewEnable = reader.ReadBoolean();
				break;
			case 10:
				equipItemDetailData.TotalOptionStepUpgradeCount = reader.ReadInt32();
				break;
			case 11:
				equipItemDetailData.RenewableOptionCount = reader.ReadInt32();
				break;
			case 12:
				equipItemDetailData.RenewCurrencyType = resolver.GetFormatterWithVerify<CurrencyType>().Deserialize(ref reader, options);
				break;
			case 13:
				equipItemDetailData.RenewPrice = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 14:
				equipItemDetailData.RenewTicketItemIndex = reader.ReadInt32();
				break;
			case 15:
				equipItemDetailData.EnableRenewSameOption = reader.ReadBoolean();
				break;
			case 16:
				equipItemDetailData.ForgeAwakeSuccessNodeTree = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 17:
				equipItemDetailData.ForgeAwakeFailNodeTree = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 18:
				equipItemDetailData.ForgeRenewSuccessNodeTree = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 19:
				equipItemDetailData.ForgeEnchantSuccessNodeTree = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 20:
				equipItemDetailData.RenewItemIndex1 = reader.ReadInt32();
				break;
			case 21:
				equipItemDetailData.RenewItemCount1 = reader.ReadInt32();
				break;
			case 22:
				equipItemDetailData.RenewItemIndex2 = reader.ReadInt32();
				break;
			case 23:
				equipItemDetailData.RenewItemCount2 = reader.ReadInt32();
				break;
			case 24:
				equipItemDetailData.RenewItemIndex3 = reader.ReadInt32();
				break;
			case 25:
				equipItemDetailData.RenewItemCount3 = reader.ReadInt32();
				break;
			case 26:
				equipItemDetailData.ExtraSkillRenewItemIndex = reader.ReadInt32();
				break;
			case 27:
				equipItemDetailData.ExtraSkillRenewItemCount = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return equipItemDetailData;
	}
}

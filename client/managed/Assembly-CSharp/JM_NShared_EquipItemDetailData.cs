using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_EquipItemDetailData
{
	public static EquipItemDetailData Parse(string s)
	{
		return Parse(s, new EquipItemDetailData());
	}

	public static EquipItemDetailData Parse(string s, EquipItemDetailData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static EquipItemDetailData Parse(IDictionary d)
	{
		return Parse(d, new EquipItemDetailData());
	}

	public static EquipItemDetailData Parse(IDictionary d, EquipItemDetailData o)
	{
		o.DetailIndex = BaseJsonMarshaler.ParseValueInt(d, "DetailIndex");
		o.EquipItemDetailType = BaseJsonMarshaler.ParseEnum<EquipItemDetailType>(d, "EquipItemDetailType");
		o.IsUnique = BaseJsonMarshaler.ParseValueBool(d, "IsUnique");
		o.MaxLevel = BaseJsonMarshaler.ParseValueInt(d, "MaxLevel");
		o.MaxLevelByHero = BaseJsonMarshaler.ParseValueBool(d, "MaxLevelByHero");
		o.ReqAwakeMaterialLevel = BaseJsonMarshaler.ParseValueInt(d, "ReqAwakeMaterialLevel");
		o.MaxStar = BaseJsonMarshaler.ParseValueInt(d, "MaxStar");
		o.OptionCount = BaseJsonMarshaler.ParseValueInt(d, "OptionCount");
		o.AllOptionOpen = BaseJsonMarshaler.ParseValueBool(d, "AllOptionOpen");
		o.RenewEnable = BaseJsonMarshaler.ParseValueBool(d, "RenewEnable");
		o.TotalOptionStepUpgradeCount = BaseJsonMarshaler.ParseValueInt(d, "TotalOptionStepUpgradeCount");
		o.RenewableOptionCount = BaseJsonMarshaler.ParseValueInt(d, "RenewableOptionCount");
		o.RenewCurrencyType = BaseJsonMarshaler.ParseEnum<CurrencyType>(d, "RenewCurrencyType");
		o.RenewPrice = BaseJsonMarshaler.ParseArrayCast(d, "RenewPrice", BaseJsonMarshaler.CastToInt);
		o.RenewTicketItemIndex = BaseJsonMarshaler.ParseValueInt(d, "RenewTicketItemIndex");
		o.EnableRenewSameOption = BaseJsonMarshaler.ParseValueBool(d, "EnableRenewSameOption");
		o.ForgeAwakeSuccessNodeTree = BaseJsonMarshaler.ParseValueString(d, "ForgeAwakeSuccessNodeTree");
		o.ForgeAwakeFailNodeTree = BaseJsonMarshaler.ParseValueString(d, "ForgeAwakeFailNodeTree");
		o.ForgeRenewSuccessNodeTree = BaseJsonMarshaler.ParseValueString(d, "ForgeRenewSuccessNodeTree");
		o.ForgeEnchantSuccessNodeTree = BaseJsonMarshaler.ParseValueString(d, "ForgeEnchantSuccessNodeTree");
		o.RenewItemIndex1 = BaseJsonMarshaler.ParseValueInt(d, "RenewItemIndex1");
		o.RenewItemCount1 = BaseJsonMarshaler.ParseValueInt(d, "RenewItemCount1");
		o.RenewItemIndex2 = BaseJsonMarshaler.ParseValueInt(d, "RenewItemIndex2");
		o.RenewItemCount2 = BaseJsonMarshaler.ParseValueInt(d, "RenewItemCount2");
		o.RenewItemIndex3 = BaseJsonMarshaler.ParseValueInt(d, "RenewItemIndex3");
		o.RenewItemCount3 = BaseJsonMarshaler.ParseValueInt(d, "RenewItemCount3");
		o.ExtraSkillRenewItemIndex = BaseJsonMarshaler.ParseValueInt(d, "ExtraSkillRenewItemIndex");
		o.ExtraSkillRenewItemCount = BaseJsonMarshaler.ParseValueInt(d, "ExtraSkillRenewItemCount");
		return o;
	}

	public static string EncodeJsonString(EquipItemDetailData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(EquipItemDetailData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("DetailIndex", o.DetailIndex.ToString());
		dictionary.Add("EquipItemDetailType", o.EquipItemDetailType.ToString());
		dictionary.Add("IsUnique", o.IsUnique.ToString());
		dictionary.Add("MaxLevel", o.MaxLevel.ToString());
		dictionary.Add("MaxLevelByHero", o.MaxLevelByHero.ToString());
		dictionary.Add("ReqAwakeMaterialLevel", o.ReqAwakeMaterialLevel.ToString());
		dictionary.Add("MaxStar", o.MaxStar.ToString());
		dictionary.Add("OptionCount", o.OptionCount.ToString());
		dictionary.Add("AllOptionOpen", o.AllOptionOpen.ToString());
		dictionary.Add("RenewEnable", o.RenewEnable.ToString());
		dictionary.Add("TotalOptionStepUpgradeCount", o.TotalOptionStepUpgradeCount.ToString());
		dictionary.Add("RenewableOptionCount", o.RenewableOptionCount.ToString());
		dictionary.Add("RenewCurrencyType", o.RenewCurrencyType.ToString());
		dictionary.Add("RenewPrice", BaseJsonMarshaler.EncodeArray(o.RenewPrice));
		dictionary.Add("RenewTicketItemIndex", o.RenewTicketItemIndex.ToString());
		dictionary.Add("EnableRenewSameOption", o.EnableRenewSameOption.ToString());
		if (o.ForgeAwakeSuccessNodeTree != null)
		{
			dictionary.Add("ForgeAwakeSuccessNodeTree", o.ForgeAwakeSuccessNodeTree);
		}
		if (o.ForgeAwakeFailNodeTree != null)
		{
			dictionary.Add("ForgeAwakeFailNodeTree", o.ForgeAwakeFailNodeTree);
		}
		if (o.ForgeRenewSuccessNodeTree != null)
		{
			dictionary.Add("ForgeRenewSuccessNodeTree", o.ForgeRenewSuccessNodeTree);
		}
		if (o.ForgeEnchantSuccessNodeTree != null)
		{
			dictionary.Add("ForgeEnchantSuccessNodeTree", o.ForgeEnchantSuccessNodeTree);
		}
		dictionary.Add("RenewItemIndex1", o.RenewItemIndex1.ToString());
		dictionary.Add("RenewItemCount1", o.RenewItemCount1.ToString());
		dictionary.Add("RenewItemIndex2", o.RenewItemIndex2.ToString());
		dictionary.Add("RenewItemCount2", o.RenewItemCount2.ToString());
		dictionary.Add("RenewItemIndex3", o.RenewItemIndex3.ToString());
		dictionary.Add("RenewItemCount3", o.RenewItemCount3.ToString());
		dictionary.Add("ExtraSkillRenewItemIndex", o.ExtraSkillRenewItemIndex.ToString());
		dictionary.Add("ExtraSkillRenewItemCount", o.ExtraSkillRenewItemCount.ToString());
		return dictionary;
	}
}

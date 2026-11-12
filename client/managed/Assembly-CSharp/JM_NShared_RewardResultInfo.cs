using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_RewardResultInfo
{
	public static RewardResultInfo Parse(string s)
	{
		return Parse(s, new RewardResultInfo());
	}

	public static RewardResultInfo Parse(string s, RewardResultInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static RewardResultInfo Parse(IDictionary d)
	{
		return Parse(d, new RewardResultInfo());
	}

	public static RewardResultInfo Parse(IDictionary d, RewardResultInfo o)
	{
		o.CurrencyResults = BaseJsonMarshaler.ParseArray(d, "CurrencyResults", JM_NShared_CurrencyResultInfo3.Parse);
		o.ItemResults = BaseJsonMarshaler.ParseArray(d, "ItemResults", JM_NShared_ItemResultInfo.Parse);
		o.EquipItemResults = BaseJsonMarshaler.ParseArray(d, "EquipItemResults", JM_NShared_EquipItemInfo.Parse);
		o.HeroInfos = BaseJsonMarshaler.ParseArray(d, "HeroInfos", JM_NShared_HeroInfo.Parse);
		o.ExpResultInfos = BaseJsonMarshaler.ParseArray(d, "ExpResultInfos", JM_NShared_ExpResultInfo.Parse);
		o.StaminaResultInfos = BaseJsonMarshaler.ParseArray(d, "StaminaResultInfos", JM_NShared_StaminaResultInfo.Parse);
		o.EquipItemBoostInfos = BaseJsonMarshaler.ParseArray(d, "EquipItemBoostInfos", JM_NShared_EquipItemBoostInfo.Parse);
		o.PetInfos = BaseJsonMarshaler.ParseArray(d, "PetInfos", JM_NShared_PetInfo.Parse);
		o.ItemTimeDurationInfos = BaseJsonMarshaler.ParseArray(d, "ItemTimeDurationInfos", JM_NShared_PlayerItemTimeDurationInfo.Parse);
		o.HeroFriendlyInfos = BaseJsonMarshaler.ParseArray(d, "HeroFriendlyInfos", JM_NShared_PlayerHeroFriendlyInfo.Parse);
		o.PlayerAnyMiscs = BaseJsonMarshaler.ParseArray(d, "PlayerAnyMiscs", JM_NShared_PlayerAnyMisc.Parse);
		return o;
	}

	public static string EncodeJsonString(RewardResultInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(RewardResultInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("CurrencyResults", BaseJsonMarshaler.EncodeArray(o.CurrencyResults, JM_NShared_CurrencyResultInfo3.EncodeJsonObject));
		dictionary.Add("ItemResults", BaseJsonMarshaler.EncodeArray(o.ItemResults, JM_NShared_ItemResultInfo.EncodeJsonObject));
		dictionary.Add("EquipItemResults", BaseJsonMarshaler.EncodeArray(o.EquipItemResults, JM_NShared_EquipItemInfo.EncodeJsonObject));
		dictionary.Add("HeroInfos", BaseJsonMarshaler.EncodeArray(o.HeroInfos, JM_NShared_HeroInfo.EncodeJsonObject));
		dictionary.Add("ExpResultInfos", BaseJsonMarshaler.EncodeArray(o.ExpResultInfos, JM_NShared_ExpResultInfo.EncodeJsonObject));
		dictionary.Add("StaminaResultInfos", BaseJsonMarshaler.EncodeArray(o.StaminaResultInfos, JM_NShared_StaminaResultInfo.EncodeJsonObject));
		dictionary.Add("EquipItemBoostInfos", BaseJsonMarshaler.EncodeArray(o.EquipItemBoostInfos, JM_NShared_EquipItemBoostInfo.EncodeJsonObject));
		dictionary.Add("PetInfos", BaseJsonMarshaler.EncodeArray(o.PetInfos, JM_NShared_PetInfo.EncodeJsonObject));
		dictionary.Add("ItemTimeDurationInfos", BaseJsonMarshaler.EncodeArray(o.ItemTimeDurationInfos, JM_NShared_PlayerItemTimeDurationInfo.EncodeJsonObject));
		dictionary.Add("HeroFriendlyInfos", BaseJsonMarshaler.EncodeArray(o.HeroFriendlyInfos, JM_NShared_PlayerHeroFriendlyInfo.EncodeJsonObject));
		dictionary.Add("PlayerAnyMiscs", BaseJsonMarshaler.EncodeArray(o.PlayerAnyMiscs, JM_NShared_PlayerAnyMisc.EncodeJsonObject));
		return dictionary;
	}
}

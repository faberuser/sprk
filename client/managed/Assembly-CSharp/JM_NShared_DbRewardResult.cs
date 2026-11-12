using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_DbRewardResult
{
	public static DbRewardResult Parse(string s)
	{
		return Parse(s, new DbRewardResult());
	}

	public static DbRewardResult Parse(string s, DbRewardResult o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static DbRewardResult Parse(IDictionary d)
	{
		return Parse(d, new DbRewardResult());
	}

	public static DbRewardResult Parse(IDictionary d, DbRewardResult o)
	{
		o.CurrencyResults = BaseJsonMarshaler.ParseArray(d, "CurrencyResults", JM_NShared_CurrencyResultInfo3.Parse);
		o.ItemResults = BaseJsonMarshaler.ParseArray(d, "ItemResults", JM_NShared_ItemResultInfo.Parse);
		o.EquipItemResults = BaseJsonMarshaler.ParseArray(d, "EquipItemResults", JM_NShared_EquipItemInfo.Parse);
		o.HeroInfos = BaseJsonMarshaler.ParseArray(d, "HeroInfos", JM_NShared_HeroInfo.Parse);
		o.ExpResultInfos = BaseJsonMarshaler.ParseArray(d, "ExpResultInfos", JM_NShared_ExpResultInfo.Parse);
		o.StaminaResultInfos = BaseJsonMarshaler.ParseArray(d, "StaminaResultInfos", JM_NShared_StaminaResultInfo.Parse);
		o.PetInfos = BaseJsonMarshaler.ParseArray(d, "PetInfos", JM_NShared_PetInfo.Parse);
		return o;
	}

	public static string EncodeJsonString(DbRewardResult o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(DbRewardResult o)
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
		dictionary.Add("PetInfos", BaseJsonMarshaler.EncodeArray(o.PetInfos, JM_NShared_PetInfo.EncodeJsonObject));
		return dictionary;
	}
}

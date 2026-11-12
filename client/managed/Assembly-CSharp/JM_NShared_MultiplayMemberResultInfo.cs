using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_MultiplayMemberResultInfo
{
	public static MultiplayMemberResultInfo Parse(string s)
	{
		return Parse(s, new MultiplayMemberResultInfo());
	}

	public static MultiplayMemberResultInfo Parse(string s, MultiplayMemberResultInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static MultiplayMemberResultInfo Parse(IDictionary d)
	{
		return Parse(d, new MultiplayMemberResultInfo());
	}

	public static MultiplayMemberResultInfo Parse(IDictionary d, MultiplayMemberResultInfo o)
	{
		o.AccountId = BaseJsonMarshaler.ParseValueLong(d, "AccountId");
		o.Nick = BaseJsonMarshaler.ParseValueString(d, "Nick");
		o.MemberIndex = BaseJsonMarshaler.ParseValueInt(d, "MemberIndex");
		o.UseStamina = BaseJsonMarshaler.ParseValueBool(d, "UseStamina");
		o.CurrencyResults = BaseJsonMarshaler.ParseArray(d, "CurrencyResults", JM_NShared_CurrencyResultInfo3.Parse);
		o.CurrencyResults2 = BaseJsonMarshaler.ParseArray(d, "CurrencyResults2", JM_NShared_CurrencyResultInfo3.Parse);
		o.ItemResults = BaseJsonMarshaler.ParseArray(d, "ItemResults", JM_NShared_ItemResultInfo.Parse);
		o.EquipItemInfos = BaseJsonMarshaler.ParseArray(d, "EquipItemInfos", JM_NShared_EquipItemInfo.Parse);
		o.ItemResults2 = BaseJsonMarshaler.ParseArray(d, "ItemResults2", JM_NShared_ItemResultInfo.Parse);
		o.EquipItemInfos2 = BaseJsonMarshaler.ParseArray(d, "EquipItemInfos2", JM_NShared_EquipItemInfo.Parse);
		o.EquipItemBoostInfos = BaseJsonMarshaler.ParseArray(d, "EquipItemBoostInfos", JM_NShared_EquipItemBoostInfo.Parse);
		o.EquipItemBoostInfos2 = BaseJsonMarshaler.ParseArray(d, "EquipItemBoostInfos2", JM_NShared_EquipItemBoostInfo.Parse);
		o.HeroExpResults = BaseJsonMarshaler.ParseArray(d, "HeroExpResults", JM_NShared_HeroExpResultInfo.Parse);
		o.FlaskResults = BaseJsonMarshaler.ParseArray(d, "FlaskResults", JM_NShared_FlaskEndCampaignResultInfo.Parse);
		o.FlaskItemResults = BaseJsonMarshaler.ParseArray(d, "FlaskItemResults", JM_NShared_ItemResultInfo.Parse);
		return o;
	}

	public static string EncodeJsonString(MultiplayMemberResultInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(MultiplayMemberResultInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("AccountId", o.AccountId.ToString());
		if (o.Nick != null)
		{
			dictionary.Add("Nick", o.Nick);
		}
		dictionary.Add("MemberIndex", o.MemberIndex.ToString());
		dictionary.Add("UseStamina", o.UseStamina.ToString());
		dictionary.Add("CurrencyResults", BaseJsonMarshaler.EncodeArray(o.CurrencyResults, JM_NShared_CurrencyResultInfo3.EncodeJsonObject));
		dictionary.Add("CurrencyResults2", BaseJsonMarshaler.EncodeArray(o.CurrencyResults2, JM_NShared_CurrencyResultInfo3.EncodeJsonObject));
		dictionary.Add("ItemResults", BaseJsonMarshaler.EncodeArray(o.ItemResults, JM_NShared_ItemResultInfo.EncodeJsonObject));
		dictionary.Add("EquipItemInfos", BaseJsonMarshaler.EncodeArray(o.EquipItemInfos, JM_NShared_EquipItemInfo.EncodeJsonObject));
		dictionary.Add("ItemResults2", BaseJsonMarshaler.EncodeArray(o.ItemResults2, JM_NShared_ItemResultInfo.EncodeJsonObject));
		dictionary.Add("EquipItemInfos2", BaseJsonMarshaler.EncodeArray(o.EquipItemInfos2, JM_NShared_EquipItemInfo.EncodeJsonObject));
		dictionary.Add("EquipItemBoostInfos", BaseJsonMarshaler.EncodeArray(o.EquipItemBoostInfos, JM_NShared_EquipItemBoostInfo.EncodeJsonObject));
		dictionary.Add("EquipItemBoostInfos2", BaseJsonMarshaler.EncodeArray(o.EquipItemBoostInfos2, JM_NShared_EquipItemBoostInfo.EncodeJsonObject));
		dictionary.Add("HeroExpResults", BaseJsonMarshaler.EncodeArray(o.HeroExpResults, JM_NShared_HeroExpResultInfo.EncodeJsonObject));
		dictionary.Add("FlaskResults", BaseJsonMarshaler.EncodeArray(o.FlaskResults, JM_NShared_FlaskEndCampaignResultInfo.EncodeJsonObject));
		dictionary.Add("FlaskItemResults", BaseJsonMarshaler.EncodeArray(o.FlaskItemResults, JM_NShared_ItemResultInfo.EncodeJsonObject));
		return dictionary;
	}
}

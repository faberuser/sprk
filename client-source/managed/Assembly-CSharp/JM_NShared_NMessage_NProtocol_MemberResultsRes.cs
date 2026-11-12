using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_MemberResultsRes
{
	public static MemberResultsRes Parse(string s)
	{
		return Parse(s, new MemberResultsRes());
	}

	public static MemberResultsRes Parse(string s, MemberResultsRes o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static MemberResultsRes Parse(IDictionary d)
	{
		return Parse(d, new MemberResultsRes());
	}

	public static MemberResultsRes Parse(IDictionary d, MemberResultsRes o)
	{
		o.UseStamina = BaseJsonMarshaler.ParseValueBool(d, "UseStamina");
		o.HeroExpResults = BaseJsonMarshaler.ParseArray(d, "HeroExpResults", JM_NShared_HeroExpResultInfo.Parse);
		o.FlaskResults = BaseJsonMarshaler.ParseArray(d, "FlaskResults", JM_NShared_FlaskEndCampaignResultInfo.Parse);
		o.FlaskItemResults = BaseJsonMarshaler.ParseArray(d, "FlaskItemResults", JM_NShared_ItemResultInfo.Parse);
		o.CurrencyResults = BaseJsonMarshaler.ParseArray(d, "CurrencyResults", JM_NShared_CurrencyResultInfo3.Parse);
		o.ItemResults = BaseJsonMarshaler.ParseArray(d, "ItemResults", JM_NShared_ItemResultInfo.Parse);
		o.EquipItemInfos = BaseJsonMarshaler.ParseArray(d, "EquipItemInfos", JM_NShared_EquipItemInfo.Parse);
		o.EquipItemBoostInfos = BaseJsonMarshaler.ParseArray(d, "EquipItemBoostInfos", JM_NShared_EquipItemBoostInfo.Parse);
		o.CurrencyResults2 = BaseJsonMarshaler.ParseArray(d, "CurrencyResults2", JM_NShared_CurrencyResultInfo3.Parse);
		o.ItemResults2 = BaseJsonMarshaler.ParseArray(d, "ItemResults2", JM_NShared_ItemResultInfo.Parse);
		o.EquipItemInfos2 = BaseJsonMarshaler.ParseArray(d, "EquipItemInfos2", JM_NShared_EquipItemInfo.Parse);
		o.EquipItemBoostInfos2 = BaseJsonMarshaler.ParseArray(d, "EquipItemBoostInfos2", JM_NShared_EquipItemBoostInfo.Parse);
		return o;
	}

	public static string EncodeJsonString(MemberResultsRes o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(MemberResultsRes o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("UseStamina", o.UseStamina.ToString());
		dictionary.Add("HeroExpResults", BaseJsonMarshaler.EncodeArray(o.HeroExpResults, JM_NShared_HeroExpResultInfo.EncodeJsonObject));
		dictionary.Add("FlaskResults", BaseJsonMarshaler.EncodeArray(o.FlaskResults, JM_NShared_FlaskEndCampaignResultInfo.EncodeJsonObject));
		dictionary.Add("FlaskItemResults", BaseJsonMarshaler.EncodeArray(o.FlaskItemResults, JM_NShared_ItemResultInfo.EncodeJsonObject));
		dictionary.Add("CurrencyResults", BaseJsonMarshaler.EncodeArray(o.CurrencyResults, JM_NShared_CurrencyResultInfo3.EncodeJsonObject));
		dictionary.Add("ItemResults", BaseJsonMarshaler.EncodeArray(o.ItemResults, JM_NShared_ItemResultInfo.EncodeJsonObject));
		dictionary.Add("EquipItemInfos", BaseJsonMarshaler.EncodeArray(o.EquipItemInfos, JM_NShared_EquipItemInfo.EncodeJsonObject));
		dictionary.Add("EquipItemBoostInfos", BaseJsonMarshaler.EncodeArray(o.EquipItemBoostInfos, JM_NShared_EquipItemBoostInfo.EncodeJsonObject));
		dictionary.Add("CurrencyResults2", BaseJsonMarshaler.EncodeArray(o.CurrencyResults2, JM_NShared_CurrencyResultInfo3.EncodeJsonObject));
		dictionary.Add("ItemResults2", BaseJsonMarshaler.EncodeArray(o.ItemResults2, JM_NShared_ItemResultInfo.EncodeJsonObject));
		dictionary.Add("EquipItemInfos2", BaseJsonMarshaler.EncodeArray(o.EquipItemInfos2, JM_NShared_EquipItemInfo.EncodeJsonObject));
		dictionary.Add("EquipItemBoostInfos2", BaseJsonMarshaler.EncodeArray(o.EquipItemBoostInfos2, JM_NShared_EquipItemBoostInfo.EncodeJsonObject));
		return dictionary;
	}
}

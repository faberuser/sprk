using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_MailReceiveResult
{
	public static MailReceiveResult Parse(string s)
	{
		return Parse(s, new MailReceiveResult());
	}

	public static MailReceiveResult Parse(string s, MailReceiveResult o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static MailReceiveResult Parse(IDictionary d)
	{
		return Parse(d, new MailReceiveResult());
	}

	public static MailReceiveResult Parse(IDictionary d, MailReceiveResult o)
	{
		o.CurrencyResults = BaseJsonMarshaler.ParseArray(d, "CurrencyResults", JM_NShared_CurrencyResultInfo3.Parse);
		o.StaminaResults = BaseJsonMarshaler.ParseArray(d, "StaminaResults", JM_NShared_StaminaResultInfo.Parse);
		o.ItemResults = BaseJsonMarshaler.ParseArray(d, "ItemResults", JM_NShared_ItemResultInfo.Parse);
		o.EquipItemResults = BaseJsonMarshaler.ParseArray(d, "EquipItemResults", JM_NShared_EquipItemInfo.Parse);
		o.HeroAddResult = BaseJsonMarshaler.ParseObject(d, "HeroAddResult", JM_NShared_HeroAddResultInfo.Parse);
		o.EventGiftPointResult = BaseJsonMarshaler.ParseObject(d, "EventGiftPointResult", JM_NShared_EventGiftPointResultInfo.Parse);
		o.GuildPointResult = BaseJsonMarshaler.ParseObject(d, "GuildPointResult", JM_NShared_GuildPointResultInfo.Parse);
		o.PetResult = BaseJsonMarshaler.ParseObject(d, "PetResult", JM_NShared_PetInfo.Parse);
		o.PlayerAnyMiscs = BaseJsonMarshaler.ParseArray(d, "PlayerAnyMiscs", JM_NShared_PlayerAnyMisc.Parse);
		return o;
	}

	public static string EncodeJsonString(MailReceiveResult o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(MailReceiveResult o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("CurrencyResults", BaseJsonMarshaler.EncodeArray(o.CurrencyResults, JM_NShared_CurrencyResultInfo3.EncodeJsonObject));
		dictionary.Add("StaminaResults", BaseJsonMarshaler.EncodeArray(o.StaminaResults, JM_NShared_StaminaResultInfo.EncodeJsonObject));
		dictionary.Add("ItemResults", BaseJsonMarshaler.EncodeArray(o.ItemResults, JM_NShared_ItemResultInfo.EncodeJsonObject));
		dictionary.Add("EquipItemResults", BaseJsonMarshaler.EncodeArray(o.EquipItemResults, JM_NShared_EquipItemInfo.EncodeJsonObject));
		dictionary.Add("HeroAddResult", JM_NShared_HeroAddResultInfo.EncodeJsonObject(o.HeroAddResult));
		dictionary.Add("EventGiftPointResult", JM_NShared_EventGiftPointResultInfo.EncodeJsonObject(o.EventGiftPointResult));
		dictionary.Add("GuildPointResult", JM_NShared_GuildPointResultInfo.EncodeJsonObject(o.GuildPointResult));
		dictionary.Add("PetResult", JM_NShared_PetInfo.EncodeJsonObject(o.PetResult));
		dictionary.Add("PlayerAnyMiscs", BaseJsonMarshaler.EncodeArray(o.PlayerAnyMiscs, JM_NShared_PlayerAnyMisc.EncodeJsonObject));
		return dictionary;
	}
}

using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_GachaItemResultInfo
{
	public static GachaItemResultInfo Parse(string s)
	{
		return Parse(s, new GachaItemResultInfo());
	}

	public static GachaItemResultInfo Parse(string s, GachaItemResultInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GachaItemResultInfo Parse(IDictionary d)
	{
		return Parse(d, new GachaItemResultInfo());
	}

	public static GachaItemResultInfo Parse(IDictionary d, GachaItemResultInfo o)
	{
		o.ItemResult = BaseJsonMarshaler.ParseObject(d, "ItemResult", JM_NShared_ItemResultInfo.Parse);
		o.EquipItemResult = BaseJsonMarshaler.ParseObject(d, "EquipItemResult", JM_NShared_EquipItemInfo.Parse);
		o.PetItemResult = BaseJsonMarshaler.ParseObject(d, "PetItemResult", JM_NShared_PetInfo.Parse);
		o.StaminaResult = BaseJsonMarshaler.ParseObject(d, "StaminaResult", JM_NShared_StaminaResultInfo.Parse);
		return o;
	}

	public static string EncodeJsonString(GachaItemResultInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GachaItemResultInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ItemResult", JM_NShared_ItemResultInfo.EncodeJsonObject(o.ItemResult));
		dictionary.Add("EquipItemResult", JM_NShared_EquipItemInfo.EncodeJsonObject(o.EquipItemResult));
		dictionary.Add("PetItemResult", JM_NShared_PetInfo.EncodeJsonObject(o.PetItemResult));
		dictionary.Add("StaminaResult", JM_NShared_StaminaResultInfo.EncodeJsonObject(o.StaminaResult));
		return dictionary;
	}
}

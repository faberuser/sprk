using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_PetGachaItemResultInfo
{
	public static PetGachaItemResultInfo Parse(string s)
	{
		return Parse(s, new PetGachaItemResultInfo());
	}

	public static PetGachaItemResultInfo Parse(string s, PetGachaItemResultInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PetGachaItemResultInfo Parse(IDictionary d)
	{
		return Parse(d, new PetGachaItemResultInfo());
	}

	public static PetGachaItemResultInfo Parse(IDictionary d, PetGachaItemResultInfo o)
	{
		o.ItemResult = BaseJsonMarshaler.ParseObject(d, "ItemResult", JM_NShared_ItemResultInfo.Parse);
		o.PetResult = BaseJsonMarshaler.ParseObject(d, "PetResult", JM_NShared_PetInfo.Parse);
		return o;
	}

	public static string EncodeJsonString(PetGachaItemResultInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PetGachaItemResultInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ItemResult", JM_NShared_ItemResultInfo.EncodeJsonObject(o.ItemResult));
		dictionary.Add("PetResult", JM_NShared_PetInfo.EncodeJsonObject(o.PetResult));
		return dictionary;
	}
}

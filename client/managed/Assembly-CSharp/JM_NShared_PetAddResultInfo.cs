using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_PetAddResultInfo
{
	public static PetAddResultInfo Parse(string s)
	{
		return Parse(s, new PetAddResultInfo());
	}

	public static PetAddResultInfo Parse(string s, PetAddResultInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PetAddResultInfo Parse(IDictionary d)
	{
		return Parse(d, new PetAddResultInfo());
	}

	public static PetAddResultInfo Parse(IDictionary d, PetAddResultInfo o)
	{
		o.PetSoulResults = BaseJsonMarshaler.ParseArray(d, "PetSoulResults", JM_NShared_ItemResultInfo.Parse);
		o.PetResult = BaseJsonMarshaler.ParseObject(d, "PetResult", JM_NShared_PetInfo.Parse);
		return o;
	}

	public static string EncodeJsonString(PetAddResultInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PetAddResultInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("PetSoulResults", BaseJsonMarshaler.EncodeArray(o.PetSoulResults, JM_NShared_ItemResultInfo.EncodeJsonObject));
		dictionary.Add("PetResult", JM_NShared_PetInfo.EncodeJsonObject(o.PetResult));
		return dictionary;
	}
}

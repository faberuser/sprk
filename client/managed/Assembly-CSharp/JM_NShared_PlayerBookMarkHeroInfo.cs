using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_PlayerBookMarkHeroInfo
{
	public static PlayerBookMarkHeroInfo Parse(string s)
	{
		return Parse(s, new PlayerBookMarkHeroInfo());
	}

	public static PlayerBookMarkHeroInfo Parse(string s, PlayerBookMarkHeroInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PlayerBookMarkHeroInfo Parse(IDictionary d)
	{
		return Parse(d, new PlayerBookMarkHeroInfo());
	}

	public static PlayerBookMarkHeroInfo Parse(IDictionary d, PlayerBookMarkHeroInfo o)
	{
		o.AccountId = BaseJsonMarshaler.ParseValueLong(d, "AccountId");
		o.BookMarkHeroIndices = BaseJsonMarshaler.ParseValueString(d, "BookMarkHeroIndices");
		return o;
	}

	public static string EncodeJsonString(PlayerBookMarkHeroInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PlayerBookMarkHeroInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("AccountId", o.AccountId.ToString());
		if (o.BookMarkHeroIndices != null)
		{
			dictionary.Add("BookMarkHeroIndices", o.BookMarkHeroIndices);
		}
		return dictionary;
	}
}

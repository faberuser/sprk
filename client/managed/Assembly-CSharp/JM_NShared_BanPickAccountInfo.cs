using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_BanPickAccountInfo
{
	public static BanPickAccountInfo Parse(string s)
	{
		return Parse(s, new BanPickAccountInfo());
	}

	public static BanPickAccountInfo Parse(string s, BanPickAccountInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static BanPickAccountInfo Parse(IDictionary d)
	{
		return Parse(d, new BanPickAccountInfo());
	}

	public static BanPickAccountInfo Parse(IDictionary d, BanPickAccountInfo o)
	{
		o.AccountId = BaseJsonMarshaler.ParseValueLong(d, "AccountId");
		o.PickHeroIndices = BaseJsonMarshaler.ParseArrayCast(d, "PickHeroIndices", BaseJsonMarshaler.CastToInt);
		o.BanHeroIndices = BaseJsonMarshaler.ParseArrayCast(d, "BanHeroIndices", BaseJsonMarshaler.CastToInt);
		return o;
	}

	public static string EncodeJsonString(BanPickAccountInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(BanPickAccountInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("AccountId", o.AccountId.ToString());
		dictionary.Add("PickHeroIndices", BaseJsonMarshaler.EncodeArray(o.PickHeroIndices));
		dictionary.Add("BanHeroIndices", BaseJsonMarshaler.EncodeArray(o.BanHeroIndices));
		return dictionary;
	}
}

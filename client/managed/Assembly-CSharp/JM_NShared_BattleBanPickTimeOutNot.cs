using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_BattleBanPickTimeOutNot
{
	public static BattleBanPickTimeOutNot Parse(string s)
	{
		return Parse(s, new BattleBanPickTimeOutNot());
	}

	public static BattleBanPickTimeOutNot Parse(string s, BattleBanPickTimeOutNot o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static BattleBanPickTimeOutNot Parse(IDictionary d)
	{
		return Parse(d, new BattleBanPickTimeOutNot());
	}

	public static BattleBanPickTimeOutNot Parse(IDictionary d, BattleBanPickTimeOutNot o)
	{
		o.AccountId = BaseJsonMarshaler.ParseValueLong(d, "AccountId");
		return o;
	}

	public static string EncodeJsonString(BattleBanPickTimeOutNot o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(BattleBanPickTimeOutNot o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("AccountId", o.AccountId.ToString());
		return dictionary;
	}
}

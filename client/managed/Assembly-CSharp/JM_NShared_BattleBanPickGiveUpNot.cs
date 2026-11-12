using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_BattleBanPickGiveUpNot
{
	public static BattleBanPickGiveUpNot Parse(string s)
	{
		return Parse(s, new BattleBanPickGiveUpNot());
	}

	public static BattleBanPickGiveUpNot Parse(string s, BattleBanPickGiveUpNot o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static BattleBanPickGiveUpNot Parse(IDictionary d)
	{
		return Parse(d, new BattleBanPickGiveUpNot());
	}

	public static BattleBanPickGiveUpNot Parse(IDictionary d, BattleBanPickGiveUpNot o)
	{
		o.AccountId = BaseJsonMarshaler.ParseValueLong(d, "AccountId");
		return o;
	}

	public static string EncodeJsonString(BattleBanPickGiveUpNot o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(BattleBanPickGiveUpNot o)
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

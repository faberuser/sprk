using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_BattleBanPickEndNot
{
	public static BattleBanPickEndNot Parse(string s)
	{
		return Parse(s, new BattleBanPickEndNot());
	}

	public static BattleBanPickEndNot Parse(string s, BattleBanPickEndNot o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static BattleBanPickEndNot Parse(IDictionary d)
	{
		return Parse(d, new BattleBanPickEndNot());
	}

	public static BattleBanPickEndNot Parse(IDictionary d, BattleBanPickEndNot o)
	{
		o.BanPickAccountInfo = BaseJsonMarshaler.ParseArray(d, "BanPickAccountInfo", JM_NShared_BanPickAccountInfo.Parse);
		o.AccountInfos = BaseJsonMarshaler.ParseArray(d, "AccountInfos", JM_NShared_MatchedAccountInfo.Parse);
		return o;
	}

	public static string EncodeJsonString(BattleBanPickEndNot o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(BattleBanPickEndNot o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("BanPickAccountInfo", BaseJsonMarshaler.EncodeArray(o.BanPickAccountInfo, JM_NShared_BanPickAccountInfo.EncodeJsonObject));
		dictionary.Add("AccountInfos", BaseJsonMarshaler.EncodeArray(o.AccountInfos, JM_NShared_MatchedAccountInfo.EncodeJsonObject));
		return dictionary;
	}
}

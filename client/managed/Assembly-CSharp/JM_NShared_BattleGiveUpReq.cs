using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_BattleGiveUpReq
{
	public static BattleGiveUpReq Parse(string s)
	{
		return Parse(s, new BattleGiveUpReq());
	}

	public static BattleGiveUpReq Parse(string s, BattleGiveUpReq o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static BattleGiveUpReq Parse(IDictionary d)
	{
		return Parse(d, new BattleGiveUpReq());
	}

	public static BattleGiveUpReq Parse(IDictionary d, BattleGiveUpReq o)
	{
		o.RequestId = BaseJsonMarshaler.ParseValueInt(d, "RequestId");
		return o;
	}

	public static string EncodeJsonString(BattleGiveUpReq o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(BattleGiveUpReq o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("RequestId", o.RequestId.ToString());
		return dictionary;
	}
}

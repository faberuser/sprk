using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_BattleStartReq
{
	public static BattleStartReq Parse(string s)
	{
		return Parse(s, new BattleStartReq());
	}

	public static BattleStartReq Parse(string s, BattleStartReq o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static BattleStartReq Parse(IDictionary d)
	{
		return Parse(d, new BattleStartReq());
	}

	public static BattleStartReq Parse(IDictionary d, BattleStartReq o)
	{
		o.RequestId = BaseJsonMarshaler.ParseValueInt(d, "RequestId");
		return o;
	}

	public static string EncodeJsonString(BattleStartReq o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(BattleStartReq o)
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

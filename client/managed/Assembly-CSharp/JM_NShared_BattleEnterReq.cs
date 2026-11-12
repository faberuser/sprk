using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_BattleEnterReq
{
	public static BattleEnterReq Parse(string s)
	{
		return Parse(s, new BattleEnterReq());
	}

	public static BattleEnterReq Parse(string s, BattleEnterReq o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static BattleEnterReq Parse(IDictionary d)
	{
		return Parse(d, new BattleEnterReq());
	}

	public static BattleEnterReq Parse(IDictionary d, BattleEnterReq o)
	{
		o.RequestId = BaseJsonMarshaler.ParseValueInt(d, "RequestId");
		return o;
	}

	public static string EncodeJsonString(BattleEnterReq o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(BattleEnterReq o)
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

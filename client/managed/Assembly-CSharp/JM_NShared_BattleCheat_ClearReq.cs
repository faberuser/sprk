using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_BattleCheat_ClearReq
{
	public static BattleCheat_ClearReq Parse(string s)
	{
		return Parse(s, new BattleCheat_ClearReq());
	}

	public static BattleCheat_ClearReq Parse(string s, BattleCheat_ClearReq o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static BattleCheat_ClearReq Parse(IDictionary d)
	{
		return Parse(d, new BattleCheat_ClearReq());
	}

	public static BattleCheat_ClearReq Parse(IDictionary d, BattleCheat_ClearReq o)
	{
		o.RequestId = BaseJsonMarshaler.ParseValueInt(d, "RequestId");
		return o;
	}

	public static string EncodeJsonString(BattleCheat_ClearReq o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(BattleCheat_ClearReq o)
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

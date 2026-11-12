using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_BattleCheat_ClearRes
{
	public static BattleCheat_ClearRes Parse(string s)
	{
		return Parse(s, new BattleCheat_ClearRes());
	}

	public static BattleCheat_ClearRes Parse(string s, BattleCheat_ClearRes o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static BattleCheat_ClearRes Parse(IDictionary d)
	{
		return Parse(d, new BattleCheat_ClearRes());
	}

	public static BattleCheat_ClearRes Parse(IDictionary d, BattleCheat_ClearRes o)
	{
		o.Result = BaseJsonMarshaler.ParseEnum<BattleCheat_ClearRes.ResultType>(d, "Result");
		o.RequestId = BaseJsonMarshaler.ParseValueInt(d, "RequestId");
		return o;
	}

	public static string EncodeJsonString(BattleCheat_ClearRes o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(BattleCheat_ClearRes o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Result", o.Result.ToString());
		dictionary.Add("RequestId", o.RequestId.ToString());
		return dictionary;
	}
}

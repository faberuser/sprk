using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_BattleLoginRes
{
	public static BattleLoginRes Parse(string s)
	{
		return Parse(s, new BattleLoginRes());
	}

	public static BattleLoginRes Parse(string s, BattleLoginRes o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static BattleLoginRes Parse(IDictionary d)
	{
		return Parse(d, new BattleLoginRes());
	}

	public static BattleLoginRes Parse(IDictionary d, BattleLoginRes o)
	{
		o.Result = BaseJsonMarshaler.ParseEnum<BattleLoginRes.ResultType>(d, "Result");
		o.RequestId = BaseJsonMarshaler.ParseValueInt(d, "RequestId");
		return o;
	}

	public static string EncodeJsonString(BattleLoginRes o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(BattleLoginRes o)
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

using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_BattleEnterRes
{
	public static BattleEnterRes Parse(string s)
	{
		return Parse(s, new BattleEnterRes());
	}

	public static BattleEnterRes Parse(string s, BattleEnterRes o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static BattleEnterRes Parse(IDictionary d)
	{
		return Parse(d, new BattleEnterRes());
	}

	public static BattleEnterRes Parse(IDictionary d, BattleEnterRes o)
	{
		o.Result = BaseJsonMarshaler.ParseEnum<BattleEnterRes.ResultType>(d, "Result");
		o.RequestId = BaseJsonMarshaler.ParseValueInt(d, "RequestId");
		return o;
	}

	public static string EncodeJsonString(BattleEnterRes o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(BattleEnterRes o)
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

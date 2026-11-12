using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_BattleWaveResumeRes
{
	public static BattleWaveResumeRes Parse(string s)
	{
		return Parse(s, new BattleWaveResumeRes());
	}

	public static BattleWaveResumeRes Parse(string s, BattleWaveResumeRes o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static BattleWaveResumeRes Parse(IDictionary d)
	{
		return Parse(d, new BattleWaveResumeRes());
	}

	public static BattleWaveResumeRes Parse(IDictionary d, BattleWaveResumeRes o)
	{
		o.Result = BaseJsonMarshaler.ParseEnum<BattleWaveResumeRes.ResultType>(d, "Result");
		o.RequestId = BaseJsonMarshaler.ParseValueInt(d, "RequestId");
		return o;
	}

	public static string EncodeJsonString(BattleWaveResumeRes o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(BattleWaveResumeRes o)
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

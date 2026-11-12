using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_BattleWaveResumeReq
{
	public static BattleWaveResumeReq Parse(string s)
	{
		return Parse(s, new BattleWaveResumeReq());
	}

	public static BattleWaveResumeReq Parse(string s, BattleWaveResumeReq o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static BattleWaveResumeReq Parse(IDictionary d)
	{
		return Parse(d, new BattleWaveResumeReq());
	}

	public static BattleWaveResumeReq Parse(IDictionary d, BattleWaveResumeReq o)
	{
		o.RequestId = BaseJsonMarshaler.ParseValueInt(d, "RequestId");
		return o;
	}

	public static string EncodeJsonString(BattleWaveResumeReq o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(BattleWaveResumeReq o)
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

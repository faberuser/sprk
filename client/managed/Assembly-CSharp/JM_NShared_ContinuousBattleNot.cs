using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_ContinuousBattleNot
{
	public static ContinuousBattleNot Parse(string s)
	{
		return Parse(s, new ContinuousBattleNot());
	}

	public static ContinuousBattleNot Parse(string s, ContinuousBattleNot o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static ContinuousBattleNot Parse(IDictionary d)
	{
		return Parse(d, new ContinuousBattleNot());
	}

	public static ContinuousBattleNot Parse(IDictionary d, ContinuousBattleNot o)
	{
		o.IsContinuousBattle = BaseJsonMarshaler.ParseValueBool(d, "IsContinuousBattle");
		return o;
	}

	public static string EncodeJsonString(ContinuousBattleNot o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(ContinuousBattleNot o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("IsContinuousBattle", o.IsContinuousBattle.ToString());
		return dictionary;
	}
}

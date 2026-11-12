using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NEvent;

public static class JM_NShared_NEvent_OpenSubPartySetting
{
	public static OpenSubPartySetting Parse(string s)
	{
		return Parse(s, new OpenSubPartySetting());
	}

	public static OpenSubPartySetting Parse(string s, OpenSubPartySetting o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static OpenSubPartySetting Parse(IDictionary d)
	{
		return Parse(d, new OpenSubPartySetting());
	}

	public static OpenSubPartySetting Parse(IDictionary d, OpenSubPartySetting o)
	{
		o.BattleType = BaseJsonMarshaler.ParseEnum<BattleType>(d, "BattleType");
		return o;
	}

	public static string EncodeJsonString(OpenSubPartySetting o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(OpenSubPartySetting o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("BattleType", o.BattleType.ToString());
		return dictionary;
	}
}

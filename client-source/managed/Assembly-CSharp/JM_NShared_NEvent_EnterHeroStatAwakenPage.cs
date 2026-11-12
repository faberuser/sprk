using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NEvent;

public static class JM_NShared_NEvent_EnterHeroStatAwakenPage
{
	public static EnterHeroStatAwakenPage Parse(string s)
	{
		return Parse(s, new EnterHeroStatAwakenPage());
	}

	public static EnterHeroStatAwakenPage Parse(string s, EnterHeroStatAwakenPage o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static EnterHeroStatAwakenPage Parse(IDictionary d)
	{
		return Parse(d, new EnterHeroStatAwakenPage());
	}

	public static EnterHeroStatAwakenPage Parse(IDictionary d, EnterHeroStatAwakenPage o)
	{
		o.AwakeType = BaseJsonMarshaler.ParseEnum<HeroAwakeType>(d, "AwakeType");
		return o;
	}

	public static string EncodeJsonString(EnterHeroStatAwakenPage o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(EnterHeroStatAwakenPage o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("AwakeType", o.AwakeType.ToString());
		return dictionary;
	}
}

using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NEvent;

public static class JM_NShared_NEvent_OpenTrialofGodking
{
	public static OpenTrialofGodking Parse(string s)
	{
		return Parse(s, new OpenTrialofGodking());
	}

	public static OpenTrialofGodking Parse(string s, OpenTrialofGodking o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static OpenTrialofGodking Parse(IDictionary d)
	{
		return Parse(d, new OpenTrialofGodking());
	}

	public static OpenTrialofGodking Parse(IDictionary d, OpenTrialofGodking o)
	{
		o.IsActive = BaseJsonMarshaler.ParseValueBool(d, "IsActive");
		return o;
	}

	public static string EncodeJsonString(OpenTrialofGodking o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(OpenTrialofGodking o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("IsActive", o.IsActive.ToString());
		return dictionary;
	}
}

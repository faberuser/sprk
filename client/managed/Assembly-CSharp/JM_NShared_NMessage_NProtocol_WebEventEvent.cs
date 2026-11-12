using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_WebEventEvent
{
	public static WebEventEvent Parse(string s)
	{
		return Parse(s, new WebEventEvent());
	}

	public static WebEventEvent Parse(string s, WebEventEvent o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static WebEventEvent Parse(IDictionary d)
	{
		return Parse(d, new WebEventEvent());
	}

	public static WebEventEvent Parse(IDictionary d, WebEventEvent o)
	{
		o.Index = BaseJsonMarshaler.ParseValueInt(d, "Index");
		o.HasAReward = BaseJsonMarshaler.ParseValueBool(d, "HasAReward");
		o.IsComplete = BaseJsonMarshaler.ParseValueBool(d, "IsComplete");
		return o;
	}

	public static string EncodeJsonString(WebEventEvent o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(WebEventEvent o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Index", o.Index.ToString());
		dictionary.Add("HasAReward", o.HasAReward.ToString());
		dictionary.Add("IsComplete", o.IsComplete.ToString());
		return dictionary;
	}
}

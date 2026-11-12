using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NContentsDefine;

public static class JM_NShared_NContentsDefine_OfficialChannel
{
	public static OfficialChannel Parse(string s)
	{
		return Parse(s, new OfficialChannel());
	}

	public static OfficialChannel Parse(string s, OfficialChannel o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static OfficialChannel Parse(IDictionary d)
	{
		return Parse(d, new OfficialChannel());
	}

	public static OfficialChannel Parse(IDictionary d, OfficialChannel o)
	{
		o.Settings = BaseJsonMarshaler.ParseDictionary<string, OfficialChannel.Setting, Dictionary<string, OfficialChannel.Setting>>(d, "Settings", JM_NShared_NContentsDefine_OfficialChannel_Setting.Parse);
		return o;
	}

	public static string EncodeJsonString(OfficialChannel o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(OfficialChannel o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Settings", BaseJsonMarshaler.EncodeDictionary(o.Settings, JM_NShared_NContentsDefine_OfficialChannel_Setting.EncodeJsonObject));
		return dictionary;
	}
}

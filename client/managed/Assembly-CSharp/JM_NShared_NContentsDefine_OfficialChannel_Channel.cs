using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NContentsDefine;

public static class JM_NShared_NContentsDefine_OfficialChannel_Channel
{
	public static OfficialChannel.Channel Parse(string s)
	{
		return Parse(s, new OfficialChannel.Channel());
	}

	public static OfficialChannel.Channel Parse(string s, OfficialChannel.Channel o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static OfficialChannel.Channel Parse(IDictionary d)
	{
		return Parse(d, new OfficialChannel.Channel());
	}

	public static OfficialChannel.Channel Parse(IDictionary d, OfficialChannel.Channel o)
	{
		o.comment = BaseJsonMarshaler.ParseValueString(d, "comment");
		o.IsEnable = BaseJsonMarshaler.ParseValueBool(d, "IsEnable");
		o.Url = BaseJsonMarshaler.ParseValueString(d, "Url");
		o.Icon = BaseJsonMarshaler.ParseValueString(d, "Icon");
		o.Name_Local = BaseJsonMarshaler.ParseValueString(d, "Name_Local");
		o.IsNewVisible = BaseJsonMarshaler.ParseValueBool(d, "IsNewVisible");
		return o;
	}

	public static string EncodeJsonString(OfficialChannel.Channel o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(OfficialChannel.Channel o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.comment != null)
		{
			dictionary.Add("comment", o.comment);
		}
		dictionary.Add("IsEnable", o.IsEnable.ToString());
		if (o.Url != null)
		{
			dictionary.Add("Url", o.Url);
		}
		if (o.Icon != null)
		{
			dictionary.Add("Icon", o.Icon);
		}
		if (o.Name_Local != null)
		{
			dictionary.Add("Name_Local", o.Name_Local);
		}
		dictionary.Add("IsNewVisible", o.IsNewVisible.ToString());
		return dictionary;
	}
}

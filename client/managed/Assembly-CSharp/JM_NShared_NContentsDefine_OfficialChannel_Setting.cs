using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NContentsDefine;

public static class JM_NShared_NContentsDefine_OfficialChannel_Setting
{
	public static OfficialChannel.Setting Parse(string s)
	{
		return Parse(s, new OfficialChannel.Setting());
	}

	public static OfficialChannel.Setting Parse(string s, OfficialChannel.Setting o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static OfficialChannel.Setting Parse(IDictionary d)
	{
		return Parse(d, new OfficialChannel.Setting());
	}

	public static OfficialChannel.Setting Parse(IDictionary d, OfficialChannel.Setting o)
	{
		o.comment = BaseJsonMarshaler.ParseValueString(d, "comment");
		o.IsDefault = BaseJsonMarshaler.ParseValueBool(d, "IsDefault");
		o.ShowLobbyButton = BaseJsonMarshaler.ParseValueBool(d, "ShowLobbyButton");
		o.NewMarkInLobbyButton = BaseJsonMarshaler.ParseValueBool(d, "NewMarkInLobbyButton");
		o.OpenInAppBrowser = BaseJsonMarshaler.ParseValueBool(d, "OpenInAppBrowser");
		o.Channels = BaseJsonMarshaler.ParseList<OfficialChannel.Channel, List<OfficialChannel.Channel>>(d, "Channels", JM_NShared_NContentsDefine_OfficialChannel_Channel.Parse);
		return o;
	}

	public static string EncodeJsonString(OfficialChannel.Setting o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(OfficialChannel.Setting o)
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
		dictionary.Add("IsDefault", o.IsDefault.ToString());
		dictionary.Add("ShowLobbyButton", o.ShowLobbyButton.ToString());
		dictionary.Add("NewMarkInLobbyButton", o.NewMarkInLobbyButton.ToString());
		dictionary.Add("OpenInAppBrowser", o.OpenInAppBrowser.ToString());
		dictionary.Add("Channels", BaseJsonMarshaler.EncodeArray(o.Channels, JM_NShared_NContentsDefine_OfficialChannel_Channel.EncodeJsonObject));
		return dictionary;
	}
}

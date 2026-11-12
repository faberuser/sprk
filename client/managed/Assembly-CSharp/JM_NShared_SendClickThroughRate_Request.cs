using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.SendClickThroughRate;

public static class JM_NShared_SendClickThroughRate_Request
{
	public static NShared.SendClickThroughRate.Request Parse(string s)
	{
		return Parse(s, new NShared.SendClickThroughRate.Request());
	}

	public static NShared.SendClickThroughRate.Request Parse(string s, NShared.SendClickThroughRate.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.SendClickThroughRate.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.SendClickThroughRate.Request());
	}

	public static NShared.SendClickThroughRate.Request Parse(IDictionary d, NShared.SendClickThroughRate.Request o)
	{
		o.AdId = BaseJsonMarshaler.ParseValueString(d, "AdId");
		o.ClickType = BaseJsonMarshaler.ParseValueInt(d, "ClickType");
		o.MetaInfo = BaseJsonMarshaler.ParseValueString(d, "MetaInfo");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.SendClickThroughRate.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.SendClickThroughRate.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.AdId != null)
		{
			dictionary.Add("AdId", o.AdId);
		}
		dictionary.Add("ClickType", o.ClickType.ToString());
		if (o.MetaInfo != null)
		{
			dictionary.Add("MetaInfo", o.MetaInfo);
		}
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}

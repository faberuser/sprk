using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.QueryMarketUrl;

public static class JM_NShared_QueryMarketUrl_Request
{
	public static NShared.QueryMarketUrl.Request Parse(string s)
	{
		return Parse(s, new NShared.QueryMarketUrl.Request());
	}

	public static NShared.QueryMarketUrl.Request Parse(string s, NShared.QueryMarketUrl.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.QueryMarketUrl.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.QueryMarketUrl.Request());
	}

	public static NShared.QueryMarketUrl.Request Parse(IDictionary d, NShared.QueryMarketUrl.Request o)
	{
		o.MarketType = BaseJsonMarshaler.ParseEnum<MarketType>(d, "MarketType");
		o.AppPlayerBuild = BaseJsonMarshaler.ParseValueInt(d, "AppPlayerBuild");
		o.Language = BaseJsonMarshaler.ParseValueString(d, "Language");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.QueryMarketUrl.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.QueryMarketUrl.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("MarketType", o.MarketType.ToString());
		dictionary.Add("AppPlayerBuild", o.AppPlayerBuild.ToString());
		if (o.Language != null)
		{
			dictionary.Add("Language", o.Language);
		}
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}

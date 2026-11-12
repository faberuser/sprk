using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_GachaSelectShopEvent
{
	public static GachaSelectShopEvent Parse(string s)
	{
		return Parse(s, new GachaSelectShopEvent());
	}

	public static GachaSelectShopEvent Parse(string s, GachaSelectShopEvent o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GachaSelectShopEvent Parse(IDictionary d)
	{
		return Parse(d, new GachaSelectShopEvent());
	}

	public static GachaSelectShopEvent Parse(IDictionary d, GachaSelectShopEvent o)
	{
		o.SelectShopNoticeInfos = BaseJsonMarshaler.ParseArray(d, "SelectShopNoticeInfos", JM_NShared_SelectShopNoticeInfo.Parse);
		return o;
	}

	public static string EncodeJsonString(GachaSelectShopEvent o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GachaSelectShopEvent o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("SelectShopNoticeInfos", BaseJsonMarshaler.EncodeArray(o.SelectShopNoticeInfos, JM_NShared_SelectShopNoticeInfo.EncodeJsonObject));
		return dictionary;
	}
}

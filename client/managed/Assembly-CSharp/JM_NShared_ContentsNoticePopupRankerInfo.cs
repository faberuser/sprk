using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_ContentsNoticePopupRankerInfo
{
	public static ContentsNoticePopupRankerInfo Parse(string s)
	{
		return Parse(s, new ContentsNoticePopupRankerInfo());
	}

	public static ContentsNoticePopupRankerInfo Parse(string s, ContentsNoticePopupRankerInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static ContentsNoticePopupRankerInfo Parse(IDictionary d)
	{
		return Parse(d, new ContentsNoticePopupRankerInfo());
	}

	public static ContentsNoticePopupRankerInfo Parse(IDictionary d, ContentsNoticePopupRankerInfo o)
	{
		o.Rank = BaseJsonMarshaler.ParseValueInt(d, "Rank");
		o.Nick = BaseJsonMarshaler.ParseValueString(d, "Nick");
		o.Score = BaseJsonMarshaler.ParseValueLong(d, "Score");
		o.AvatarHeroIndex = BaseJsonMarshaler.ParseValueInt(d, "AvatarHeroIndex");
		o.CountryCode = BaseJsonMarshaler.ParseValueString(d, "CountryCode");
		o.ServerGroup = BaseJsonMarshaler.ParseValueString(d, "ServerGroup");
		return o;
	}

	public static string EncodeJsonString(ContentsNoticePopupRankerInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(ContentsNoticePopupRankerInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Rank", o.Rank.ToString());
		if (o.Nick != null)
		{
			dictionary.Add("Nick", o.Nick);
		}
		dictionary.Add("Score", o.Score.ToString());
		dictionary.Add("AvatarHeroIndex", o.AvatarHeroIndex.ToString());
		if (o.CountryCode != null)
		{
			dictionary.Add("CountryCode", o.CountryCode);
		}
		if (o.ServerGroup != null)
		{
			dictionary.Add("ServerGroup", o.ServerGroup);
		}
		return dictionary;
	}
}

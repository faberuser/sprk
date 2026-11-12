using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetContentsNoticePopupRankerList;

public static class JM_NShared_GetContentsNoticePopupRankerList_Request
{
	public static NShared.GetContentsNoticePopupRankerList.Request Parse(string s)
	{
		return Parse(s, new NShared.GetContentsNoticePopupRankerList.Request());
	}

	public static NShared.GetContentsNoticePopupRankerList.Request Parse(string s, NShared.GetContentsNoticePopupRankerList.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetContentsNoticePopupRankerList.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetContentsNoticePopupRankerList.Request());
	}

	public static NShared.GetContentsNoticePopupRankerList.Request Parse(IDictionary d, NShared.GetContentsNoticePopupRankerList.Request o)
	{
		o.ContentsType = BaseJsonMarshaler.ParseEnum<ContentsNoticeType>(d, "ContentsType");
		o.SeasonIndex = BaseJsonMarshaler.ParseValueInt(d, "SeasonIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetContentsNoticePopupRankerList.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetContentsNoticePopupRankerList.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ContentsType", o.ContentsType.ToString());
		dictionary.Add("SeasonIndex", o.SeasonIndex.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}

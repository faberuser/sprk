using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.SetReviewPopup;

public static class JM_NShared_SetReviewPopup_Request
{
	public static NShared.SetReviewPopup.Request Parse(string s)
	{
		return Parse(s, new NShared.SetReviewPopup.Request());
	}

	public static NShared.SetReviewPopup.Request Parse(string s, NShared.SetReviewPopup.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.SetReviewPopup.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.SetReviewPopup.Request());
	}

	public static NShared.SetReviewPopup.Request Parse(IDictionary d, NShared.SetReviewPopup.Request o)
	{
		o.ReviewType = BaseJsonMarshaler.ParseEnum<AppReviewPopupType>(d, "ReviewType");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.SetReviewPopup.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.SetReviewPopup.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ReviewType", o.ReviewType.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}

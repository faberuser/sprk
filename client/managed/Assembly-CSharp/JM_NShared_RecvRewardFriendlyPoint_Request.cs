using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.RecvRewardFriendlyPoint;

public static class JM_NShared_RecvRewardFriendlyPoint_Request
{
	public static NShared.RecvRewardFriendlyPoint.Request Parse(string s)
	{
		return Parse(s, new NShared.RecvRewardFriendlyPoint.Request());
	}

	public static NShared.RecvRewardFriendlyPoint.Request Parse(string s, NShared.RecvRewardFriendlyPoint.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.RecvRewardFriendlyPoint.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.RecvRewardFriendlyPoint.Request());
	}

	public static NShared.RecvRewardFriendlyPoint.Request Parse(IDictionary d, NShared.RecvRewardFriendlyPoint.Request o)
	{
		o.HeroIndex = BaseJsonMarshaler.ParseValueInt(d, "HeroIndex");
		o.Step = BaseJsonMarshaler.ParseValueInt(d, "Step");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.RecvRewardFriendlyPoint.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.RecvRewardFriendlyPoint.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("HeroIndex", o.HeroIndex.ToString());
		dictionary.Add("Step", o.Step.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}

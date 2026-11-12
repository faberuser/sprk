using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.CompleteSubQuest;

public static class JM_NShared_CompleteSubQuest_Request
{
	public static NShared.CompleteSubQuest.Request Parse(string s)
	{
		return Parse(s, new NShared.CompleteSubQuest.Request());
	}

	public static NShared.CompleteSubQuest.Request Parse(string s, NShared.CompleteSubQuest.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.CompleteSubQuest.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.CompleteSubQuest.Request());
	}

	public static NShared.CompleteSubQuest.Request Parse(IDictionary d, NShared.CompleteSubQuest.Request o)
	{
		o.SubQuestIndices = BaseJsonMarshaler.ParseArrayCast(d, "SubQuestIndices", BaseJsonMarshaler.CastToInt);
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.CompleteSubQuest.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.CompleteSubQuest.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("SubQuestIndices", BaseJsonMarshaler.EncodeArray(o.SubQuestIndices));
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}

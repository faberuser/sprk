using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.HeroLimitBreakExpUp;

public static class JM_NShared_HeroLimitBreakExpUp_Request
{
	public static NShared.HeroLimitBreakExpUp.Request Parse(string s)
	{
		return Parse(s, new NShared.HeroLimitBreakExpUp.Request());
	}

	public static NShared.HeroLimitBreakExpUp.Request Parse(string s, NShared.HeroLimitBreakExpUp.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.HeroLimitBreakExpUp.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.HeroLimitBreakExpUp.Request());
	}

	public static NShared.HeroLimitBreakExpUp.Request Parse(IDictionary d, NShared.HeroLimitBreakExpUp.Request o)
	{
		o.HeroIndex = BaseJsonMarshaler.ParseValueInt(d, "HeroIndex");
		o.ExpItemIndices = BaseJsonMarshaler.ParseArrayCast(d, "ExpItemIndices", BaseJsonMarshaler.CastToInt);
		o.ExpItemCounts = BaseJsonMarshaler.ParseArrayCast(d, "ExpItemCounts", BaseJsonMarshaler.CastToInt);
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.HeroLimitBreakExpUp.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.HeroLimitBreakExpUp.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("HeroIndex", o.HeroIndex.ToString());
		dictionary.Add("ExpItemIndices", BaseJsonMarshaler.EncodeArray(o.ExpItemIndices));
		dictionary.Add("ExpItemCounts", BaseJsonMarshaler.EncodeArray(o.ExpItemCounts));
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}

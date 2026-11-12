using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.SetGuildArenaDeck;

public static class JM_NShared_SetGuildArenaDeck_Request
{
	public static NShared.SetGuildArenaDeck.Request Parse(string s)
	{
		return Parse(s, new NShared.SetGuildArenaDeck.Request());
	}

	public static NShared.SetGuildArenaDeck.Request Parse(string s, NShared.SetGuildArenaDeck.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.SetGuildArenaDeck.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.SetGuildArenaDeck.Request());
	}

	public static NShared.SetGuildArenaDeck.Request Parse(IDictionary d, NShared.SetGuildArenaDeck.Request o)
	{
		o.SeasonIndex = BaseJsonMarshaler.ParseValueInt(d, "SeasonIndex");
		o.HeroIndices1 = BaseJsonMarshaler.ParseArrayCast(d, "HeroIndices1", BaseJsonMarshaler.CastToInt);
		o.HeroIndices2 = BaseJsonMarshaler.ParseArrayCast(d, "HeroIndices2", BaseJsonMarshaler.CastToInt);
		o.HeroIndices3 = BaseJsonMarshaler.ParseArrayCast(d, "HeroIndices3", BaseJsonMarshaler.CastToInt);
		o.HeroIndices4 = BaseJsonMarshaler.ParseArrayCast(d, "HeroIndices4", BaseJsonMarshaler.CastToInt);
		o.HeroIndices5 = BaseJsonMarshaler.ParseArrayCast(d, "HeroIndices5", BaseJsonMarshaler.CastToInt);
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.SetGuildArenaDeck.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.SetGuildArenaDeck.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("SeasonIndex", o.SeasonIndex.ToString());
		dictionary.Add("HeroIndices1", BaseJsonMarshaler.EncodeArray(o.HeroIndices1));
		dictionary.Add("HeroIndices2", BaseJsonMarshaler.EncodeArray(o.HeroIndices2));
		dictionary.Add("HeroIndices3", BaseJsonMarshaler.EncodeArray(o.HeroIndices3));
		dictionary.Add("HeroIndices4", BaseJsonMarshaler.EncodeArray(o.HeroIndices4));
		dictionary.Add("HeroIndices5", BaseJsonMarshaler.EncodeArray(o.HeroIndices5));
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}

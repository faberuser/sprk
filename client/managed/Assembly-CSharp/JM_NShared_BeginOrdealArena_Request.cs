using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.BeginOrdealArena;

public static class JM_NShared_BeginOrdealArena_Request
{
	public static NShared.BeginOrdealArena.Request Parse(string s)
	{
		return Parse(s, new NShared.BeginOrdealArena.Request());
	}

	public static NShared.BeginOrdealArena.Request Parse(string s, NShared.BeginOrdealArena.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.BeginOrdealArena.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.BeginOrdealArena.Request());
	}

	public static NShared.BeginOrdealArena.Request Parse(IDictionary d, NShared.BeginOrdealArena.Request o)
	{
		o.NodeIndex = BaseJsonMarshaler.ParseValueInt(d, "NodeIndex");
		o.HeroIndices = BaseJsonMarshaler.ParseArrayCast(d, "HeroIndices", BaseJsonMarshaler.CastToInt);
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.BeginOrdealArena.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.BeginOrdealArena.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("NodeIndex", o.NodeIndex.ToString());
		dictionary.Add("HeroIndices", BaseJsonMarshaler.EncodeArray(o.HeroIndices));
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}

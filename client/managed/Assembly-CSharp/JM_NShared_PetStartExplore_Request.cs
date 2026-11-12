using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.PetStartExplore;

public static class JM_NShared_PetStartExplore_Request
{
	public static NShared.PetStartExplore.Request Parse(string s)
	{
		return Parse(s, new NShared.PetStartExplore.Request());
	}

	public static NShared.PetStartExplore.Request Parse(string s, NShared.PetStartExplore.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.PetStartExplore.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.PetStartExplore.Request());
	}

	public static NShared.PetStartExplore.Request Parse(IDictionary d, NShared.PetStartExplore.Request o)
	{
		o.ExploreIndex = BaseJsonMarshaler.ParseValueInt(d, "ExploreIndex");
		o.DeckIndex = BaseJsonMarshaler.ParseValueInt(d, "DeckIndex");
		o.PetIndices = BaseJsonMarshaler.ParseArrayCast(d, "PetIndices", BaseJsonMarshaler.CastToInt);
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.PetStartExplore.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.PetStartExplore.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ExploreIndex", o.ExploreIndex.ToString());
		dictionary.Add("DeckIndex", o.DeckIndex.ToString());
		dictionary.Add("PetIndices", BaseJsonMarshaler.EncodeArray(o.PetIndices));
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}

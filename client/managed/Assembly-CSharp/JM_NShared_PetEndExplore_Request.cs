using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.PetEndExplore;

public static class JM_NShared_PetEndExplore_Request
{
	public static NShared.PetEndExplore.Request Parse(string s)
	{
		return Parse(s, new NShared.PetEndExplore.Request());
	}

	public static NShared.PetEndExplore.Request Parse(string s, NShared.PetEndExplore.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.PetEndExplore.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.PetEndExplore.Request());
	}

	public static NShared.PetEndExplore.Request Parse(IDictionary d, NShared.PetEndExplore.Request o)
	{
		o.ExploreIndex = BaseJsonMarshaler.ParseValueInt(d, "ExploreIndex");
		o.DeckIndex = BaseJsonMarshaler.ParseValueInt(d, "DeckIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.PetEndExplore.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.PetEndExplore.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ExploreIndex", o.ExploreIndex.ToString());
		dictionary.Add("DeckIndex", o.DeckIndex.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}

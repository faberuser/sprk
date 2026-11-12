using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.PetCancelExplore;

public static class JM_NShared_PetCancelExplore_Request
{
	public static NShared.PetCancelExplore.Request Parse(string s)
	{
		return Parse(s, new NShared.PetCancelExplore.Request());
	}

	public static NShared.PetCancelExplore.Request Parse(string s, NShared.PetCancelExplore.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.PetCancelExplore.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.PetCancelExplore.Request());
	}

	public static NShared.PetCancelExplore.Request Parse(IDictionary d, NShared.PetCancelExplore.Request o)
	{
		o.ExploreIndex = BaseJsonMarshaler.ParseValueInt(d, "ExploreIndex");
		o.DeckIndex = BaseJsonMarshaler.ParseValueInt(d, "DeckIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.PetCancelExplore.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.PetCancelExplore.Request o)
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

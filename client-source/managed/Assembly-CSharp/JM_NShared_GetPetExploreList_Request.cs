using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetPetExploreList;

public static class JM_NShared_GetPetExploreList_Request
{
	public static NShared.GetPetExploreList.Request Parse(string s)
	{
		return Parse(s, new NShared.GetPetExploreList.Request());
	}

	public static NShared.GetPetExploreList.Request Parse(string s, NShared.GetPetExploreList.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetPetExploreList.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetPetExploreList.Request());
	}

	public static NShared.GetPetExploreList.Request Parse(IDictionary d, NShared.GetPetExploreList.Request o)
	{
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetPetExploreList.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetPetExploreList.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}

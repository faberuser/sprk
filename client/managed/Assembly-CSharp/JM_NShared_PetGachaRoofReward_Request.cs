using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.PetGachaRoofReward;

public static class JM_NShared_PetGachaRoofReward_Request
{
	public static NShared.PetGachaRoofReward.Request Parse(string s)
	{
		return Parse(s, new NShared.PetGachaRoofReward.Request());
	}

	public static NShared.PetGachaRoofReward.Request Parse(string s, NShared.PetGachaRoofReward.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.PetGachaRoofReward.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.PetGachaRoofReward.Request());
	}

	public static NShared.PetGachaRoofReward.Request Parse(IDictionary d, NShared.PetGachaRoofReward.Request o)
	{
		o.GachaIndex = BaseJsonMarshaler.ParseValueInt(d, "GachaIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.PetGachaRoofReward.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.PetGachaRoofReward.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("GachaIndex", o.GachaIndex.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}

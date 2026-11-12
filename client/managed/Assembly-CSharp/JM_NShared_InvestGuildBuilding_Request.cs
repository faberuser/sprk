using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.InvestGuildBuilding;

public static class JM_NShared_InvestGuildBuilding_Request
{
	public static NShared.InvestGuildBuilding.Request Parse(string s)
	{
		return Parse(s, new NShared.InvestGuildBuilding.Request());
	}

	public static NShared.InvestGuildBuilding.Request Parse(string s, NShared.InvestGuildBuilding.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.InvestGuildBuilding.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.InvestGuildBuilding.Request());
	}

	public static NShared.InvestGuildBuilding.Request Parse(IDictionary d, NShared.InvestGuildBuilding.Request o)
	{
		o.BuildingIndex = BaseJsonMarshaler.ParseValueInt(d, "BuildingIndex");
		o.Wood = BaseJsonMarshaler.ParseValueInt(d, "Wood");
		o.Stone = BaseJsonMarshaler.ParseValueInt(d, "Stone");
		o.Metal = BaseJsonMarshaler.ParseValueInt(d, "Metal");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.InvestGuildBuilding.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.InvestGuildBuilding.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("BuildingIndex", o.BuildingIndex.ToString());
		dictionary.Add("Wood", o.Wood.ToString());
		dictionary.Add("Stone", o.Stone.ToString());
		dictionary.Add("Metal", o.Metal.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}

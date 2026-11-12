using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.LevelUpGuildBuilding;

public static class JM_NShared_LevelUpGuildBuilding_Request
{
	public static NShared.LevelUpGuildBuilding.Request Parse(string s)
	{
		return Parse(s, new NShared.LevelUpGuildBuilding.Request());
	}

	public static NShared.LevelUpGuildBuilding.Request Parse(string s, NShared.LevelUpGuildBuilding.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.LevelUpGuildBuilding.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.LevelUpGuildBuilding.Request());
	}

	public static NShared.LevelUpGuildBuilding.Request Parse(IDictionary d, NShared.LevelUpGuildBuilding.Request o)
	{
		o.BuildingIndex = BaseJsonMarshaler.ParseValueInt(d, "BuildingIndex");
		o.BuildingLevel = BaseJsonMarshaler.ParseValueByte(d, "BuildingLevel");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.LevelUpGuildBuilding.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.LevelUpGuildBuilding.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("BuildingIndex", o.BuildingIndex.ToString());
		dictionary.Add("BuildingLevel", o.BuildingLevel.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}

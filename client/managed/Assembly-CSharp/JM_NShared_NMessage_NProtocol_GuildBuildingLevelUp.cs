using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_GuildBuildingLevelUp
{
	public static GuildBuildingLevelUp Parse(string s)
	{
		return Parse(s, new GuildBuildingLevelUp());
	}

	public static GuildBuildingLevelUp Parse(string s, GuildBuildingLevelUp o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GuildBuildingLevelUp Parse(IDictionary d)
	{
		return Parse(d, new GuildBuildingLevelUp());
	}

	public static GuildBuildingLevelUp Parse(IDictionary d, GuildBuildingLevelUp o)
	{
		o.GuildBuildingIndex = BaseJsonMarshaler.ParseValueInt(d, "GuildBuildingIndex");
		o.GuildBuildingLevel = BaseJsonMarshaler.ParseValueByte(d, "GuildBuildingLevel");
		o.UpdatedTime = BaseJsonMarshaler.ParseValueString(d, "UpdatedTime");
		o.Wood = BaseJsonMarshaler.ParseValueInt(d, "Wood");
		o.Stone = BaseJsonMarshaler.ParseValueInt(d, "Stone");
		o.Metal = BaseJsonMarshaler.ParseValueInt(d, "Metal");
		return o;
	}

	public static string EncodeJsonString(GuildBuildingLevelUp o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GuildBuildingLevelUp o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("GuildBuildingIndex", o.GuildBuildingIndex.ToString());
		dictionary.Add("GuildBuildingLevel", o.GuildBuildingLevel.ToString());
		if (o.UpdatedTime != null)
		{
			dictionary.Add("UpdatedTime", o.UpdatedTime);
		}
		dictionary.Add("Wood", o.Wood.ToString());
		dictionary.Add("Stone", o.Stone.ToString());
		dictionary.Add("Metal", o.Metal.ToString());
		return dictionary;
	}
}

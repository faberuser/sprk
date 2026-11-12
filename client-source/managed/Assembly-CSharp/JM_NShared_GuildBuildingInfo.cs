using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_GuildBuildingInfo
{
	public static GuildBuildingInfo Parse(string s)
	{
		return Parse(s, new GuildBuildingInfo());
	}

	public static GuildBuildingInfo Parse(string s, GuildBuildingInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GuildBuildingInfo Parse(IDictionary d)
	{
		return Parse(d, new GuildBuildingInfo());
	}

	public static GuildBuildingInfo Parse(IDictionary d, GuildBuildingInfo o)
	{
		o.BuildingIndex = BaseJsonMarshaler.ParseValueInt(d, "BuildingIndex");
		o.BuildingLevel = BaseJsonMarshaler.ParseValueByte(d, "BuildingLevel");
		o.Wood = BaseJsonMarshaler.ParseValueInt(d, "Wood");
		o.Stone = BaseJsonMarshaler.ParseValueInt(d, "Stone");
		o.Metal = BaseJsonMarshaler.ParseValueInt(d, "Metal");
		o.UpdatedTime = BaseJsonMarshaler.ParseValueString(d, "UpdatedTime");
		return o;
	}

	public static string EncodeJsonString(GuildBuildingInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GuildBuildingInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("BuildingIndex", o.BuildingIndex.ToString());
		dictionary.Add("BuildingLevel", o.BuildingLevel.ToString());
		dictionary.Add("Wood", o.Wood.ToString());
		dictionary.Add("Stone", o.Stone.ToString());
		dictionary.Add("Metal", o.Metal.ToString());
		if (o.UpdatedTime != null)
		{
			dictionary.Add("UpdatedTime", o.UpdatedTime);
		}
		return dictionary;
	}
}

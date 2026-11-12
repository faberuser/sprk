using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_GuildBuildingResource
{
	public static GuildBuildingResource Parse(string s)
	{
		return Parse(s, new GuildBuildingResource());
	}

	public static GuildBuildingResource Parse(string s, GuildBuildingResource o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GuildBuildingResource Parse(IDictionary d)
	{
		return Parse(d, new GuildBuildingResource());
	}

	public static GuildBuildingResource Parse(IDictionary d, GuildBuildingResource o)
	{
		o.Wood = BaseJsonMarshaler.ParseValueInt(d, "Wood");
		o.Stone = BaseJsonMarshaler.ParseValueInt(d, "Stone");
		o.Metal = BaseJsonMarshaler.ParseValueInt(d, "Metal");
		return o;
	}

	public static string EncodeJsonString(GuildBuildingResource o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GuildBuildingResource o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Wood", o.Wood.ToString());
		dictionary.Add("Stone", o.Stone.ToString());
		dictionary.Add("Metal", o.Metal.ToString());
		return dictionary;
	}
}

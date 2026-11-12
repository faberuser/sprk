using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_BeginCampaign
{
	public static BeginCampaign Parse(string s)
	{
		return Parse(s, new BeginCampaign());
	}

	public static BeginCampaign Parse(string s, BeginCampaign o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static BeginCampaign Parse(IDictionary d)
	{
		return Parse(d, new BeginCampaign());
	}

	public static BeginCampaign Parse(IDictionary d, BeginCampaign o)
	{
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		return o;
	}

	public static string EncodeJsonString(BeginCampaign o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(BeginCampaign o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ChapterIndex", o.ChapterIndex.ToString());
		dictionary.Add("DungeonIndex", o.DungeonIndex.ToString());
		return dictionary;
	}
}

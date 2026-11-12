using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.SearchRaidRoom;

public static class JM_NShared_SearchRaidRoom_Request
{
	public static NShared.SearchRaidRoom.Request Parse(string s)
	{
		return Parse(s, new NShared.SearchRaidRoom.Request());
	}

	public static NShared.SearchRaidRoom.Request Parse(string s, NShared.SearchRaidRoom.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.SearchRaidRoom.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.SearchRaidRoom.Request());
	}

	public static NShared.SearchRaidRoom.Request Parse(IDictionary d, NShared.SearchRaidRoom.Request o)
	{
		o.RaidIndex = BaseJsonMarshaler.ParseValueInt(d, "RaidIndex");
		o.StartRaidMonsterLevel = BaseJsonMarshaler.ParseValueInt(d, "StartRaidMonsterLevel");
		o.GoalIndex = BaseJsonMarshaler.ParseValueByte(d, "GoalIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.SearchRaidRoom.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.SearchRaidRoom.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("RaidIndex", o.RaidIndex.ToString());
		dictionary.Add("StartRaidMonsterLevel", o.StartRaidMonsterLevel.ToString());
		dictionary.Add("GoalIndex", o.GoalIndex.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}

using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_RaidRoomDestroyed
{
	public static RaidRoomDestroyed Parse(string s)
	{
		return Parse(s, new RaidRoomDestroyed());
	}

	public static RaidRoomDestroyed Parse(string s, RaidRoomDestroyed o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static RaidRoomDestroyed Parse(IDictionary d)
	{
		return Parse(d, new RaidRoomDestroyed());
	}

	public static RaidRoomDestroyed Parse(IDictionary d, RaidRoomDestroyed o)
	{
		return o;
	}

	public static string EncodeJsonString(RaidRoomDestroyed o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(RaidRoomDestroyed o)
	{
		return new Dictionary<string, object>();
	}
}
